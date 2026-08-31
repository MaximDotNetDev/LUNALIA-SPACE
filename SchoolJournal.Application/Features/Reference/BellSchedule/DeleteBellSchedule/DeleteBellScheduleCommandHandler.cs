using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.DeleteBellSchedule;

public sealed class DeleteBellScheduleCommandHandler(
    IBellScheduleRepository bellScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteBellScheduleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteBellScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await bellScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "BellSchedule.NotFound",
                description: "Розклад дзвінків не знайдено або він вже видалений.");
        }

        auditContext.TrackOldState(oldState);

        var result = await bellScheduleRepository.DeleteAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Failure(
                code: "BellSchedule.DeleteError",
                description: "Не вдалося видалити розклад дзвінків.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}