using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.DeleteTeachingAssignment;

public sealed class DeleteTeachingAssignmentCommandHandler(
    ITeachingAssignmentRepository teachingAssignmentRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteTeachingAssignmentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteTeachingAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await teachingAssignmentRepository.DeleteAsync(
            request.AssignmentId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "TeachingAssignment.ConcurrencyOrNotFound",
                description: "Запис вже був змінений, видалений іншим користувачем, або не існує. Оновіть сторінку.");
        }

        auditContext.TrackOldState(oldState);

        return Result.Success;
    }
}