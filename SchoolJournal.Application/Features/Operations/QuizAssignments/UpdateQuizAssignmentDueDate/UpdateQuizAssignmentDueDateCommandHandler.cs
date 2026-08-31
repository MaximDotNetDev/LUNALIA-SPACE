using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.UpdateQuizAssignmentDueDate;

public sealed class UpdateQuizAssignmentDueDateCommandHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateQuizAssignmentDueDateCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateQuizAssignmentDueDateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignment = await quizAssignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        if (assignment is null || assignment.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.NotFound",
                description: "Призначення тесту не знайдено або його було видалено.");
        }

        var quiz = await quizRepository.GetByIdAsync(assignment.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.QuizNotFound",
                description: "Пов'язаний тест не знайдено або його було видалено.");
        }

        var userRole = currentUserService.GetUserRole();
        var userId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && quiz.TeacherId != userId)
        {
            return Error.Forbidden(
                code: "QuizAssignment.Forbidden",
                description: "Ви не є автором цього тесту і не маєте права змінювати терміни його призначення.");
        }

        if (request.DueDate.HasValue && request.DueDate < assignment.AssignedDate)
        {
            return Error.Conflict(
                code: "QuizAssignment.InvalidDueDate",
                description: "Кінцевий термін здачі не може бути ранішим за дату призначення тесту.");
        }

        auditContext.TrackOldState(assignment);

        var updatedAssignment = assignment with
        {
            DueDate = request.DueDate,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var updatedResult = await quizAssignmentRepository.UpdateAsync(updatedAssignment, cancellationToken).ConfigureAwait(false);
        if (updatedResult is null)
        {
            return Error.Conflict(
                code: "QuizAssignment.ConcurrencyConflict",
                description: "Дані були змінені іншим користувачем. Будь ласка, оновіть сторінку і спробуйте знову.");
        }

        auditContext.TrackNewState(updatedResult);

        return Result.Success;
    }
}