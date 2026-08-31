using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.DeleteQuizQuestion;

public sealed record DeleteQuizQuestionCommand(
    Guid QuestionId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;