using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFillwordFromPdf;

public sealed record GenerateFillwordFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int WordCount,
    int PointsPerWord
) : IRequest<ErrorOr<GeneratedQuizResponse>>;