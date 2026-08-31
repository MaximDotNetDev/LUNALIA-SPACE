using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizById;

public sealed record GetQuizByIdQuery(
    Guid QuizId
) : IRequest<ErrorOr<QuizDetailResponse>>;