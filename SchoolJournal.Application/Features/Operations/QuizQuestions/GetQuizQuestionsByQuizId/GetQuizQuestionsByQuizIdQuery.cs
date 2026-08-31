using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionsByQuizId;

public sealed record GetQuizQuestionsByQuizIdQuery(
    Guid QuizId,
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<QuizQuestionResponse>>>;