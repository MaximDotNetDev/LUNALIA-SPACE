using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.CreateTeachingAssignment;

public sealed class CreateTeachingAssignmentCommandHandler(
    ITeachingAssignmentRepository teachingAssignmentRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateTeachingAssignmentCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateTeachingAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await teachingAssignmentRepository.ExistsAsync(request.TeacherId, request.SubjectId, request.ClassId, request.SubgroupId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeachingAssignment.Duplicate",
                description: "Таке призначення для цього класу (чи підгрупи) вже існує.");
        }

        var assignment = new TeachingAssignment
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            ClassId = request.ClassId,
            SubgroupId = request.SubgroupId
        };

        var assignmentId = await teachingAssignmentRepository.AddAsync(assignment, cancellationToken).ConfigureAwait(false);

        var newState = await teachingAssignmentRepository.GetByIdAsync(assignmentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return assignmentId;
    }
}