using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.DeleteQuizAssignment;

public sealed class DeleteQuizAssignmentCommandHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<DeleteQuizAssignmentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteQuizAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignment = await quizAssignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        if (assignment is null || assignment.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.NotFound",
                description: "Призначення тесту не знайдено або вже було видалено.");
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
                description: "Ви не є автором цього тесту і не маєте права скасовувати його призначення.");
        }

        auditContext.TrackOldState(assignment);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);
        var deletedResult = await quizAssignmentRepository.DeleteAsync(request.AssignmentId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (deletedResult is null)
        {
            return Error.Conflict(
                code: "QuizAssignment.ConcurrencyConflict",
                description: "Дані були змінені іншим користувачем. Будь ласка, оновіть сторінку і спробуйте знову.");
        }

        auditContext.TrackNewState(deletedResult);

        return Result.Success;
    }
}