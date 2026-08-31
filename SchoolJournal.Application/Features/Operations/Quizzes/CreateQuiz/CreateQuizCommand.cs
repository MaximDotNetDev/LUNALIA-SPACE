using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Quizzes.CreateQuiz;

public sealed record CreateQuizCommand(
    Guid TeacherId,
    Guid SubjectId,
    string Title
) : IRequest<ErrorOr<Guid>>;