using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.CreateFixedSchedule;

public sealed class CreateFixedScheduleCommandHandler(
    IFixedScheduleRepository fixedScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateFixedScheduleCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateFixedScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dayOfWeekInt = (int)request.DayOfWeek;

        if (await fixedScheduleRepository.HasOverlappingRoomAsync(dayOfWeekInt, request.PeriodId, request.RoomId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "FixedSchedule.RoomOccupied",
                description: "Обраний кабінет вже зайнятий у цей день та період.");
        }

        if (await fixedScheduleRepository.HasOverlappingAssignmentAsync(dayOfWeekInt, request.PeriodId, request.AssignmentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "FixedSchedule.AssignmentOccupied",
                description: "Для цього навчального призначення вже існує заняття у цей день та період.");
        }

        var schedule = new FixedSchedule
        {
            DayOfWeek = request.DayOfWeek,
            PeriodId = request.PeriodId,
            AssignmentId = request.AssignmentId,
            RoomId = request.RoomId
        };

        var scheduleId = await fixedScheduleRepository.AddAsync(schedule, cancellationToken).ConfigureAwait(false);

        var newState = await fixedScheduleRepository.GetByIdAsync(scheduleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return scheduleId;
    }
}