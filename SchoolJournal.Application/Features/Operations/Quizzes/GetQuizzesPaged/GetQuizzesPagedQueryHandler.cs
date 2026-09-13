using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesPaged;

public sealed class GetQuizzesPagedQueryHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService) // Додано сервіс для перевірки користувача
    : IRequestHandler<GetQuizzesPagedQuery, ErrorOr<PagedResponse<QuizResponse>>>
{
    public async Task<ErrorOr<PagedResponse<QuizResponse>>> Handle(GetQuizzesPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        IEnumerable<Quiz> items;
        int totalCount;

        if (userRole == RoleType.Teacher)
        {
            // Якщо це вчитель — віддаємо тільки його тести
            var currentTeacherId = await currentUserService.GetTeacherIdAsync(cancellationToken).ConfigureAwait(false);

            var result = await quizRepository.GetPagedByTeacherIdAsync(
                currentTeacherId,
                request.PageRequest.Skip,
                request.PageRequest.PageSize,
                cancellationToken).ConfigureAwait(false);

            items = result.Items;
            totalCount = result.TotalCount;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                items = items.Where(q => q.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                totalCount = items.Count();
            }
        }
        else
        {
            // Для Адмінів та Директорів віддаємо всі тести
            var result = await quizRepository.GetPagedAsync(
                request.SearchTerm,
                request.PageRequest.Skip,
                request.PageRequest.PageSize,
                cancellationToken).ConfigureAwait(false);

            items = result.Items;
            totalCount = result.TotalCount;
        }

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