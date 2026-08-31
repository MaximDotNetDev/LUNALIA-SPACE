using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFillwordFromPdf;

public sealed class GenerateFillwordFromPdfCommandHandler(
    IPdfTextExtractionService pdfExtractor,
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateFillwordFromPdfCommand, ErrorOr<GeneratedQuizResponse>>
{
    private const int MaxTextLengthForAi = 300_000;

    // Кешуємо налаштування серіалізації для оптимізації продуктивності
    private static readonly System.Text.Json.JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateFillwordFromPdfCommand request, CancellationToken cancellationToken)
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

        var aiResult = await aiQuizGenerator.GenerateFillwordAsync(
                    extractedText,
                    request.WordCount,
                    cancellationToken).ConfigureAwait(false);

        if (aiResult.IsError) return aiResult.Errors;

        // Будуємо сітку локально на бекенді
        var fillwordContent = SchoolJournal.Application.Common.Utils.FillwordGenerator.Generate(aiResult.Value.Words, request.WordCount);

        if (fillwordContent.Words.Count < 3)
        {
            return Error.Validation(
                code: "Fillword.GenerationFailed",
                description: "Не вдалося побудувати сітку філворду з достатньою кількістю слів.");
        }

        var contentJson = System.Text.Json.JsonSerializer.Serialize(fillwordContent, s_jsonOptions);

        var totalPoints = request.PointsPerWord * fillwordContent.Words.Count;
        var title = string.IsNullOrWhiteSpace(aiResult.Value.Title) ? "Філворд" : aiResult.Value.Title;

        return new GeneratedQuizResponse(
            title,
            [
                new GeneratedQuestionResponse(
                    0,
                    $"Знайдіть заховані слова (слова згинаються під кутом 90 градусів вправо або вниз). Кількість слів: {fillwordContent.Words.Count}",
                    10,
                    contentJson,
                    totalPoints)
            ]);
    }
}