using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces; // Тут тепер живе IPdfTextExtractionService
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFromPdf;

public sealed class GenerateMultipleChoiceFromPdfCommandHandler(
    IPdfTextExtractionService pdfExtractor,
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateMultipleChoiceFromPdfCommand, ErrorOr<GeneratedQuizResponse>>
{
    private const int MaxTextLengthForAi = 300_000;

    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateMultipleChoiceFromPdfCommand request, CancellationToken cancellationToken)
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
            return Error.Validation(
                code: "Pdf.EmptyText",
                description: "Не вдалося розпізнати текст у завантаженому PDF файлі або на вказаних сторінках.");
        }

        if (extractedText.Length < 50)
        {
            return Error.Validation(
                code: "Pdf.TextTooShort",
                description: "Текст у PDF занадто короткий для якісної генерації тесту (мінімум 50 символів).");
        }

        if (extractedText.Length > MaxTextLengthForAi)
        {
            extractedText = extractedText[..MaxTextLengthForAi];
        }

        return await aiQuizGenerator.GenerateMultipleChoiceAsync(
            extractedText,
            request.TotalQuestions,
            request.MultiAnswerCount,
            request.JudgmentCount,
            request.PointsPerQuestion,
            cancellationToken).ConfigureAwait(false);
    }
}