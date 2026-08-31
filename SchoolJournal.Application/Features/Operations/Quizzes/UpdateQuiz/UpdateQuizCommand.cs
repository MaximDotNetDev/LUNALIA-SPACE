using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Quizzes.UpdateQuiz;

public sealed record UpdateQuizQuestionCommand(
    Guid? QuestionId,
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);

public sealed record UpdateQuizCommand(
    Guid QuizId,
    Guid SubjectId,
    string Title,
    string RowVersionBase64,
    IReadOnlyCollection<UpdateQuizQuestionCommand> Questions
) : IRequest<ErrorOr<Updated>>;