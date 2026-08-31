using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetActiveQuizAssignmentsByClassId;

public sealed class GetActiveQuizAssignmentsByClassIdQueryHandler(
    IQuizAssignmentRepository quizAssignmentRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetActiveQuizAssignmentsByClassIdQuery, ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<QuizAssignmentResponse>>> Handle(
        GetActiveQuizAssignmentsByClassIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var teacherId = currentUserService.GetUserId();
            var hasAccess = await quizAssignmentRepository.TeacherTeachesClassAsync(teacherId, request.ClassId, cancellationToken).ConfigureAwait(false);

            if (!hasAccess)
            {
                return Error.Forbidden(
                    code: "QuizAssignment.ClassAccessForbidden",
                    description: "Ви не викладаєте у цьому класі, тому не маєте доступу до перегляду його призначень.");
            }
        }

        var assignments = await quizAssignmentRepository.GetActiveByClassIdAsync(request.ClassId, cancellationToken).ConfigureAwait(false);

        var response = assignments.Select(a => new QuizAssignmentResponse(
                    a.AssignmentId,
                    a.QuizId,
                    a.ClassId,
                    a.AssignedDate,
                    a.DueDate,
                    Convert.ToBase64String(a.RowVersion.ToArray()),
                    a.QuizTitle, 
                    a.ClassName   
                )).ToList().AsReadOnly();

        return response;
    }
}