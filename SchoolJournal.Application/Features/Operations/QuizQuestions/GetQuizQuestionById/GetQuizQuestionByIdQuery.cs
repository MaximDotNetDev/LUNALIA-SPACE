using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionById;

public sealed record GetQuizQuestionByIdQuery(
    Guid QuestionId
) : IRequest<ErrorOr<QuizQuestionResponse>>;