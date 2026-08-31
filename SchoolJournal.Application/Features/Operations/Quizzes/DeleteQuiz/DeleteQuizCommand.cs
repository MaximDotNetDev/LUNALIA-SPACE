using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Quizzes.DeleteQuiz;

public sealed record DeleteQuizCommand(
    Guid QuizId,
    string RowVersionBase64
) : IRequest<ErrorOr<Deleted>>;