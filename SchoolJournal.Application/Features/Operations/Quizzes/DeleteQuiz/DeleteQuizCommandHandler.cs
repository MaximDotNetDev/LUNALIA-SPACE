using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Quizzes.DeleteQuiz;

public sealed class DeleteQuizCommandHandler(
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<DeleteQuizCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingQuiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);

        if (existingQuiz is null || existingQuiz.IsDeleted)
        {
            return Error.NotFound(
                code: "Quiz.NotFound",
                description: $"Тест з ID '{request.QuizId}' не знайдено або він вже видалений.");
        }

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && existingQuiz.TeacherId != currentUserId)
        {
            return Error.Forbidden(
                code: "Quiz.OwnershipViolation",
                description: "Ви не маєте прав на видалення чужого тесту.");
        }

        auditContext.TrackOldState(existingQuiz);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var deletedQuiz = await quizRepository.DeleteAsync(request.QuizId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (deletedQuiz is null)
        {
            return Error.Conflict(
                code: "Quiz.ConcurrencyConflict",
                description: "Не вдалося видалити тест. Можливо, дані були змінені іншим користувачем.");
        }

        auditContext.TrackNewState(deletedQuiz);

        return Result.Deleted;
    }
}