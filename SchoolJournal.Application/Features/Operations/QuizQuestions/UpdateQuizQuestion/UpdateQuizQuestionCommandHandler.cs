using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.UpdateQuizQuestion;

public sealed class UpdateQuizQuestionCommandHandler(
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateQuizQuestionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingQuestion = await quizQuestionRepository.GetByIdAsync(request.QuestionId, cancellationToken).ConfigureAwait(false);
        if (existingQuestion is null || existingQuestion.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizQuestion.NotFound",
                description: $"Питання з ідентифікатором '{request.QuestionId}' не знайдено або видалено.");
        }

        var quiz = await quizRepository.GetByIdAsync(existingQuestion.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Батьківський тест для цього питання не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var currentUserId = currentUserService.GetUserId();
            if (quiz.TeacherId != currentUserId)
            {
                return Error.Forbidden(
                    code: "Quiz.OwnershipViolation",
                    description: "У вас немає прав на редагування питань цього тесту, оскільки ви не є його автором.");
            }
        }

        auditContext.TrackOldState(existingQuestion);

        var byteRowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var updatedQuestion = existingQuestion with
        {
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            ContentJson = request.ContentJson,
            Points = request.Points,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = byteRowVersion
        };

        var updatedResult = await quizQuestionRepository.UpdateAsync(updatedQuestion, cancellationToken).ConfigureAwait(false);
        if (updatedResult is null)
        {
            return Error.Conflict(
                code: "QuizQuestion.ConcurrencyConflict",
                description: "Дані були змінені або видалені іншим користувачем. Оновіть сторінку та спробуйте знову.");
        }

        auditContext.TrackNewState(updatedResult);

        return Result.Success;
    }
}