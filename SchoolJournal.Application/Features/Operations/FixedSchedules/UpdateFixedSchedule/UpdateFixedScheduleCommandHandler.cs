using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.UpdateFixedSchedule;

public sealed class UpdateFixedScheduleCommandHandler(
    IFixedScheduleRepository fixedScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateFixedScheduleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateFixedScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await fixedScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);
        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "FixedSchedule.NotFound",
                description: "Запис у розкладі не знайдено або він був видалений.");
        }

        auditContext.TrackOldState(oldState);

        var dayOfWeekInt = (int)request.DayOfWeek;

        if (await fixedScheduleRepository.HasOverlappingRoomExcludingIdAsync(dayOfWeekInt, request.PeriodId, request.RoomId, request.ScheduleId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "FixedSchedule.RoomOccupied",
                description: "Обраний кабінет вже зайнятий у цей день та період іншим заняттям.");
        }

        if (await fixedScheduleRepository.HasOverlappingAssignmentExcludingIdAsync(dayOfWeekInt, request.PeriodId, request.AssignmentId, request.ScheduleId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "FixedSchedule.AssignmentOccupied",
                description: "Для цього навчального призначення вже існує інше заняття у цей день та період.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var scheduleToUpdate = new FixedSchedule
        {
            ScheduleId = request.ScheduleId,
            DayOfWeek = request.DayOfWeek,
            PeriodId = request.PeriodId,
            AssignmentId = request.AssignmentId,
            RoomId = request.RoomId,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersionBytes
        };

        var updatedSchedule = await fixedScheduleRepository.UpdateAsync(scheduleToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedSchedule is null)
        {
            return Error.Conflict(
                code: "Concurrency.UpdateFailed",
                description: "Дані були змінені іншим користувачем. Оновіть сторінку та спробуйте ще раз.");
        }

        auditContext.TrackNewState(updatedSchedule);

        return Result.Success;
    }
}