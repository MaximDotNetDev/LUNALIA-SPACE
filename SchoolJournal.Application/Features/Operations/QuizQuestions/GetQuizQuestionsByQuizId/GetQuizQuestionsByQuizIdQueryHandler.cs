using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionsByQuizId;

public sealed class GetQuizQuestionsByQuizIdQueryHandler(
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizQuestionsByQuizIdQuery, ErrorOr<PagedResponse<QuizQuestionResponse>>>
{
    public async Task<ErrorOr<PagedResponse<QuizQuestionResponse>>> Handle(GetQuizQuestionsByQuizIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест з ідентифікатором '{request.QuizId}' не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var currentUserId = currentUserService.GetUserId();
            if (quiz.TeacherId != currentUserId)
            {
                return Error.Forbidden(
                    code: "Quiz.OwnershipViolation",
                    description: "У вас немає прав на перегляд питань цього тесту, оскільки ви не є його автором.");
            }
        }

        var (items, totalCount) = await quizQuestionRepository.GetPagedByQuizIdAsync(
            request.QuizId,
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(question => new QuizQuestionResponse(
            question.QuestionId,
            question.QuizId,
            question.OrderIndex,
            question.QuestionText,
            question.QuestionType,
            question.ContentJson,
            question.Points,
            question.CreatedAt,
            question.UpdatedAt,
            Convert.ToBase64String(question.RowVersion.ToArray())
        ));

        return new PagedResponse<QuizQuestionResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}