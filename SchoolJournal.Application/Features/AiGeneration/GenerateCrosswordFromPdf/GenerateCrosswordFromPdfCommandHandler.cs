using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using System.Text.Json;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateCrosswordFromPdf;

public sealed class GenerateCrosswordFromPdfCommandHandler(
    IPdfTextExtractionService pdfExtractor,
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateCrosswordFromPdfCommand, ErrorOr<GeneratedQuizResponse>>
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private const int MaxTextLengthForAi = 300_000;

    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateCrosswordFromPdfCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string extractedText;
        try
        {
            extractedText = pdfExtractor.ExtractTextFromPdf(request.PdfBytes, request.StartPage, request.EndPage);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation(code: "Pdf.ExtractionFailed", description: $"Помилка обробки PDF: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Error.Validation(code: "Pdf.EmptyContent", description: "Не вдалося розпізнати текст на вказаних сторінках.");
        }

        if (extractedText.Length < 50)
        {
            return Error.Validation(code: "Pdf.TextTooShort", description: "Текст занадто короткий для якісної генерації.");
        }

        if (extractedText.Length > MaxTextLengthForAi)
        {
            extractedText = extractedText[..MaxTextLengthForAi];
        }

        var aiResult = await aiQuizGenerator.GenerateCrosswordAsync(
                    extractedText,
                    request.WordCount,
                    cancellationToken).ConfigureAwait(false);

        if (aiResult.IsError) return aiResult.Errors;

        // Передаємо request.WordCount у генератор сітки, щоб він відсік прихований запас
        var crosswordContent = SchoolJournal.Application.Common.Utils.CrosswordGenerator.Generate(aiResult.Value.Words, request.WordCount);

        if (crosswordContent.Words.Count < 3)
        {
            return Error.Validation(
                code: "Crossword.GenerationFailed",
                description: "Не вдалося знайти достатньо перетинів слів для створення кросворда.");
        }

        // Використовуємо закешований серіалізатор
        var contentJson = JsonSerializer.Serialize(crosswordContent, s_jsonOptions);

        var totalPoints = request.PointsPerWord * crosswordContent.Words.Count;
        var title = string.IsNullOrWhiteSpace(aiResult.Value.Title) ? "Кросворд" : aiResult.Value.Title;

        var response = new GeneratedQuizResponse(
                    title,
                    [
                        new GeneratedQuestionResponse(
                    0,
                    $"Розгадайте кросворд. Кількість слів: {crosswordContent.Words.Count}",
                    9,
                    contentJson,
                    totalPoints)
                    ]);
        
        return response;
    }
}