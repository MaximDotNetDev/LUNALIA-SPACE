using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFillInTheBlankFromPdf;

public sealed class GenerateFillInTheBlankFromPdfCommandHandler(
    IPdfTextExtractionService pdfExtractor,
    IAiQuizGenerator aiQuizGenerator)
    : IRequestHandler<GenerateFillInTheBlankFromPdfCommand, ErrorOr<GeneratedQuizResponse>>
{
    private const int MaxTextLengthForAi = 300_000;

    public async Task<ErrorOr<GeneratedQuizResponse>> Handle(GenerateFillInTheBlankFromPdfCommand request, CancellationToken cancellationToken)
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

        return await aiQuizGenerator.GenerateFillInTheBlankAsync(
            extractedText,
            request.QuestionCount,
            request.PointsPerQuestion,
            cancellationToken).ConfigureAwait(false);
    }
}