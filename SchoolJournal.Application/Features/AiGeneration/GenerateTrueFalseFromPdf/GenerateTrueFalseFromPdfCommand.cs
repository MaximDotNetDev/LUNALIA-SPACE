using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateTrueFalseFromPdf;

public sealed record GenerateTrueFalseFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int QuestionCount,
    int PointsPerQuestion
) : IRequest<ErrorOr<GeneratedQuizResponse>>;