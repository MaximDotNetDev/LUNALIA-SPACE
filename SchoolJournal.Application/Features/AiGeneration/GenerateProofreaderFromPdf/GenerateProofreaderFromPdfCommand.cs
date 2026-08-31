using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateProofreaderFromPdf;

public sealed record GenerateProofreaderFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int QuestionCount,
    int PointsPerQuestion
) : IRequest<ErrorOr<GeneratedQuizResponse>>;