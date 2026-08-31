using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateCrosswordFromPdf;

public sealed record GenerateCrosswordFromPdfCommand(
    IReadOnlyCollection<byte> PdfBytes,
    int? StartPage,
    int? EndPage,
    int WordCount,
    int PointsPerWord
) : IRequest<ErrorOr<GeneratedQuizResponse>>;