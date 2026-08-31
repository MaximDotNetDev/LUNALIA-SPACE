using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFillInTheBlankFromPdf;

public sealed record GenerateFillInTheBlankFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int QuestionCount,
    int PointsPerQuestion
) : IRequest<ErrorOr<GeneratedQuizResponse>>;