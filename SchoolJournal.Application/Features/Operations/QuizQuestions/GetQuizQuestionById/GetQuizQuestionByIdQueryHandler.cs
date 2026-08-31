using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionById;

public sealed class GetQuizQuestionByIdQueryHandler(
    IQuizQuestionRepository quizQuestionRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizQuestionByIdQuery, ErrorOr<QuizQuestionResponse>>
{
    public async Task<ErrorOr<QuizQuestionResponse>> Handle(GetQuizQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var question = await quizQuestionRepository.GetByIdAsync(request.QuestionId, cancellationToken).ConfigureAwait(false);
        if (question is null || question.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizQuestion.NotFound",
                description: $"Питання з ідентифікатором '{request.QuestionId}' не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var quiz = await quizRepository.GetByIdAsync(question.QuizId, cancellationToken).ConfigureAwait(false);
            if (quiz is null || quiz.IsDeleted)
            {
                return Error.NotFound(
                    code: "Quiz.NotFound",
                    description: "Батьківський тест не знайдено.");
            }

            var currentUserId = currentUserService.GetUserId();
            if (quiz.TeacherId != currentUserId)
            {
                return Error.Forbidden(
                    code: "QuizQuestion.OwnershipViolation",
                    description: "У вас немає прав на перегляд цього питання, оскільки ви не є автором тесту.");
            }
        }

        return new QuizQuestionResponse(
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
        );
    }
}