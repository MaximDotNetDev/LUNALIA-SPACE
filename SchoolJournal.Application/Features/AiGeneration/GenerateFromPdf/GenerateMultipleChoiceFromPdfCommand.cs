using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFromPdf;

public sealed record GenerateMultipleChoiceFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int TotalQuestions,
    int MultiAnswerCount,
    int JudgmentCount,
    int PointsPerQuestion
) : IRequest<ErrorOr<GeneratedQuizResponse>>;