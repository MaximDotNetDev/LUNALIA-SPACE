using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.CreateQuizQuestion;

public sealed record CreateQuizQuestionCommand(
    Guid QuizId,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
) : IRequest<ErrorOr<Guid>>;