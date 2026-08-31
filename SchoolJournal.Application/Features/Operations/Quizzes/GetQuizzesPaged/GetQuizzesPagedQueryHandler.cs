using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesPaged;

public sealed class GetQuizzesPagedQueryHandler(
    IQuizRepository quizRepository)
    : IRequestHandler<GetQuizzesPagedQuery, ErrorOr<PagedResponse<QuizResponse>>>
{
    public async Task<ErrorOr<PagedResponse<QuizResponse>>> Handle(GetQuizzesPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await quizRepository.GetPagedAsync(
            request.SearchTerm,
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var quizResponses = items.Select(quiz => new QuizResponse(
            quiz.QuizId,
            quiz.TeacherId,
            quiz.SubjectId,
            quiz.Title,
            Convert.ToBase64String(quiz.RowVersion.ToArray()),
            quiz.CreatedAt,
            quiz.UpdatedAt
        )).ToList();

        return new PagedResponse<QuizResponse>(
            quizResponses,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}