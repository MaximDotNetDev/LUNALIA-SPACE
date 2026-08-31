using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizById;

public sealed class GetQuizByIdQueryHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizByIdQuery, ErrorOr<QuizDetailResponse>>
{
    public async Task<ErrorOr<QuizDetailResponse>> Handle(GetQuizByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (quiz, questions) = await quizRepository.GetWithQuestionsByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);

        if (quiz is null)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест з ID '{request.QuizId}' не знайдено або він видалений.");
        }

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && quiz.TeacherId != currentUserId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Ви не маєте прав на перегляд цього тесту.");
        }

        var questionResponses = questions.Select(q => new QuizQuestionResponse(
            q.QuestionId,
            q.OrderIndex,
            q.QuestionText,
            q.QuestionType,
            q.ContentJson,
            q.Points,
            Convert.ToBase64String(q.RowVersion.ToArray())
        )).ToList();

        return new QuizDetailResponse(
            quiz.QuizId,
            quiz.TeacherId,
            quiz.SubjectId,
            quiz.Title,
            Convert.ToBase64String(quiz.RowVersion.ToArray()),
            quiz.CreatedAt,
            quiz.UpdatedAt,
            questionResponses
        );
    }
}