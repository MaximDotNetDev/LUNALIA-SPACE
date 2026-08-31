using System.Text.Json;
using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Contracts.DTOs.AiGeneration;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateTrueFalseFromPdf;

public sealed class GenerateTrueFalseFromPdfCommandHandler(
    IPdfTextExtractionService pdfExtractor,
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateTrueFalseFromPdfCommand, ErrorOr<GeneratedQuizResponse>>
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const int MaxTextLengthForAi = 300_000;

    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateTrueFalseFromPdfCommand request, CancellationToken cancellationToken)
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
            return Error.Validation(code: "Pdf.TextTooShort", description: "Текст у PDF занадто короткий для якісної генерації (мінімум 50 символів).");
        }

        if (extractedText.Length > MaxTextLengthForAi)
        {
            extractedText = extractedText[..MaxTextLengthForAi];
        }

        var aiResult = await aiQuizGenerator.GenerateTrueFalseAsync(
            extractedText,
            request.QuestionCount,
            cancellationToken).ConfigureAwait(false);

        if (aiResult.IsError) return aiResult.Errors;

        var aiQuiz = aiResult.Value;
        var generatedQuestions = new List<GeneratedQuestionResponse>();
        int orderIndex = 0;

        foreach (var q in aiQuiz.Questions)
        {
            // Безпечна серіалізація на стороні бекенду (Fail Fast захист від кривих рядків ШІ)
            var contentObj = new { isTrue = q.IsTrue, explanation = q.Explanation ?? "Пояснення відсутнє." };
            var contentJson = JsonSerializer.Serialize(contentObj, s_jsonOptions);

            generatedQuestions.Add(new GeneratedQuestionResponse(
                orderIndex++,
                string.IsNullOrWhiteSpace(q.Statement) ? "Помилка генерації твердження" : q.Statement,
                4, // QuestionType = 4 (TrueFalse)
                contentJson,
                request.PointsPerQuestion
            ));
        }

        var title = string.IsNullOrWhiteSpace(aiQuiz.Title) ? "Завдання: Так чи ні" : aiQuiz.Title;

        return new GeneratedQuizResponse(title, generatedQuestions);
    }
}