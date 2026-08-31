using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.CreateFixedSchedule;

public sealed record CreateFixedScheduleCommand(
    SchoolDayOfWeek DayOfWeek,
    Guid PeriodId,
    Guid AssignmentId,
    Guid RoomId
) : IRequest<ErrorOr<Guid>>;