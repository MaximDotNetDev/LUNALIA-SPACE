using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesByTeacher;

public sealed class GetQuizzesByTeacherQueryHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizzesByTeacherQuery, ErrorOr<PagedResponse<QuizResponse>>>
{
    public async Task<ErrorOr<PagedResponse<QuizResponse>>> Handle(GetQuizzesByTeacherQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && request.TeacherId != currentUserId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Ви не маєте прав на перегляд тестів іншого вчителя.");
        }

        if (!await quizRepository.TeacherExistsAsync(request.TeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Quiz.TeacherNotFound",
                description: $"Вчителя з ID '{request.TeacherId}' не знайдено.");
        }

        var (items, totalCount) = await quizRepository.GetPagedByTeacherIdAsync(
            request.TeacherId,
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