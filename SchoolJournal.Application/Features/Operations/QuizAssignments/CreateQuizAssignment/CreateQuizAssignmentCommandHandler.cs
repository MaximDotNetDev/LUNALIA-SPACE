using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.CreateQuizAssignment;

public sealed class CreateQuizAssignmentCommandHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<CreateQuizAssignmentCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateQuizAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken).ConfigureAwait(false);
        if (quiz is null || quiz.IsDeleted)
        {
            return Error.NotFound(
                code: "QuizAssignment.QuizNotFound",
                description: "Вказаний тест не знайдено або його було видалено.");
        }

        var userRole = currentUserService.GetUserRole();
        var userId = currentUserService.GetUserId();

        if (userRole == RoleType.Teacher && quiz.TeacherId != userId)
        {
            return Error.Forbidden(
                code: "QuizAssignment.Forbidden",
                description: "Ви не є автором цього тесту і не маєте права його призначати.");
        }

        if (userRole == RoleType.Teacher)
        {
            var teachesClass = await quizAssignmentRepository.TeacherTeachesClassAsync(userId, request.ClassId, cancellationToken).ConfigureAwait(false);
            if (!teachesClass)
            {
                return Error.Forbidden(
                    code: "QuizAssignment.ClassAccessDenied",
                    description: "Ви не можете призначити тест класу, в якому не викладаєте.");
            }
        }

        var assignment = new QuizAssignment
        {
            QuizId = request.QuizId,
            ClassId = request.ClassId,
            AssignedDate = DateTimeOffset.UtcNow,
            DueDate = request.DueDate
        };

        var assignmentId = await quizAssignmentRepository.AddAsync(assignment, cancellationToken).ConfigureAwait(false);

        var newState = await quizAssignmentRepository.GetByIdAsync(assignmentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return assignmentId;
    }
}