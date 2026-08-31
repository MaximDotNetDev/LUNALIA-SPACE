using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Quizzes.SaveGeneratedQuiz;

public sealed record SaveGeneratedQuizCommand(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    string Title,
    IReadOnlyCollection<SaveGeneratedQuizQuestionCommandItem> Questions
) : IRequest<ErrorOr<Guid>>;

public sealed record SaveGeneratedQuizQuestionCommandItem(
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);