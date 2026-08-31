using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.UpdateQuizQuestion;

public sealed record UpdateQuizQuestionCommand(
    Guid QuestionId,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;