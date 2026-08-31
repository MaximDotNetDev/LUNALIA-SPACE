using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.DeleteQuizQuestion;

public sealed class DeleteQuizQuestionCommandHandler(
    IQuizRepository quizRepository,
    IQuizQuestionRepository quizQuestionRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<DeleteQuizQuestionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteQuizQuestionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingQuestion = await quizQuestionRepository.GetByIdAsync(request.QuestionId, cancellationToken).ConfigureAwait(false);
        if (existingQuestion is null || existingQuestion.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizQuestion.NotFound",
                description: $"Питання з ідентифікатором '{request.QuestionId}' не знайдено або вже видалено.");
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
                    description: "У вас немає прав на видалення питань цього тесту, оскільки ви не є його автором.");
            }
        }

        auditContext.TrackOldState(existingQuestion);

        var byteRowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var deletedResult = await quizQuestionRepository.DeleteAsync(request.QuestionId, byteRowVersion, cancellationToken).ConfigureAwait(false);
        if (deletedResult is null)
        {
            return Error.Conflict(
                code: "QuizQuestion.ConcurrencyConflict",
                description: "Не вдалося видалити питання. Дані були змінені іншим користувачем. Оновіть сторінку.");
        }

        auditContext.TrackNewState(deletedResult);

        return Result.Success;
    }
}