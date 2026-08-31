using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.UpdateTeachingAssignment;

public sealed class UpdateTeachingAssignmentCommandHandler(
    ITeachingAssignmentRepository teachingAssignmentRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateTeachingAssignmentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateTeachingAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await teachingAssignmentRepository.ExistsExcludingIdAsync(request.TeacherId, request.SubjectId, request.ClassId, request.SubgroupId, request.AssignmentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "TeachingAssignment.Duplicate",
                description: "Таке призначення для цього класу (чи підгрупи) вже існує в іншому записі.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var assignment = new TeachingAssignment
        {
            AssignmentId = request.AssignmentId,
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            ClassId = request.ClassId,
            SubgroupId = request.SubgroupId,
            RowVersion = rowVersionBytes
        };

        var oldState = await teachingAssignmentRepository.UpdateAsync(assignment, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "TeachingAssignment.Concurrency",
                description: "Запис було змінено або видалено іншим користувачем. Оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await teachingAssignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}