using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.ToggleTeachingAssignmentStatus;

public sealed class ToggleTeachingAssignmentStatusCommandHandler(
    ITeachingAssignmentRepository teachingAssignmentRepository,
    IAuditContext auditContext)
    : IRequestHandler<ToggleTeachingAssignmentStatusCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ToggleTeachingAssignmentStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await teachingAssignmentRepository.ToggleStatusAsync(
            request.AssignmentId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "TeachingAssignment.ConcurrencyOrNotFound",
                description: "Запис змінено, видалено або не знайдено. Оновіть дані.");
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