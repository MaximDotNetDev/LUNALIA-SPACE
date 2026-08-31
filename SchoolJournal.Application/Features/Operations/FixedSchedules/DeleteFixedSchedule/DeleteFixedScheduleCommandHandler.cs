using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.DeleteFixedSchedule;

public sealed class DeleteFixedScheduleCommandHandler(
    IFixedScheduleRepository fixedScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteFixedScheduleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteFixedScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedule = await fixedScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);

        if (schedule is null || schedule.IsDeleted)
        {
            return Error.NotFound(
                code: "FixedSchedule.NotFound",
                description: "Запис у розкладі не знайдено або він вже видалений.");
        }

        auditContext.TrackOldState(schedule);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var deletedSchedule = await fixedScheduleRepository.DeleteAsync(
            request.ScheduleId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (deletedSchedule is null)
        {
            return Error.Conflict(
                code: "Concurrency.DeleteFailed",
                description: "Не вдалося видалити запис. Можливо, він був змінений іншим користувачем.");
        }

        auditContext.TrackNewState(deletedSchedule);

        return Result.Success;
    }
}