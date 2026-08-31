using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateQuizCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingQuiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);

        if (existingQuiz is null || existingQuiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест з ID '{request.QuizId}' не знайдено або він видалений.");
        }

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && existingQuiz.TeacherId != currentUserId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Ви не маєте прав на редагування чужого тесту.");
        }

        if (existingQuiz.SubjectId != request.SubjectId &&
            !await quizRepository.SubjectExistsAsync(request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Quiz.SubjectNotFound",
                description: $"Предмета з ID '{request.SubjectId}' не знайдено.");
        }

        auditContext.TrackOldState(existingQuiz);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var updateModel = new Quiz
        {
            QuizId = request.QuizId,
            SubjectId = request.SubjectId,
            Title = request.Title,
            RowVersion = rowVersionBytes
        };

        var questionsToSync = request.Questions.Select(q => new QuizQuestion
        {
            QuestionId = q.QuestionId ?? Guid.Empty,
            QuizId = request.QuizId,
            OrderIndex = q.OrderIndex,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            ContentJson = q.ContentJson,
            Points = q.Points
        }).ToList();

        var updatedQuiz = await quizRepository.UpdateWithQuestionsAsync(updateModel, questionsToSync, cancellationToken).ConfigureAwait(false);

        if (updatedQuiz is null)
        {
            return Error.Conflict(
                code: "Quiz.ConcurrencyConflict",
                description: "Дані тесту були змінені іншим користувачем. Оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackNewState(updatedQuiz);

        return Result.Updated;
    }
}