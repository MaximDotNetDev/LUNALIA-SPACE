using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

namespace SchoolJournal.Application.Features.Operations.QuizSubmissions.SubmitQuiz;

public sealed record SubmitQuizCommand(
    Guid AssignmentId,
    IReadOnlyCollection<QuizAnswerDto> Answers)
    : IRequest<ErrorOr<SubmitQuizResponse>>;