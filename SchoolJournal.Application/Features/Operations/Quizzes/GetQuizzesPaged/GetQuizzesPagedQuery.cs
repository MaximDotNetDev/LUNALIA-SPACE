using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesPaged;

public sealed record GetQuizzesPagedQuery(
    string? SearchTerm,
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<QuizResponse>>>;