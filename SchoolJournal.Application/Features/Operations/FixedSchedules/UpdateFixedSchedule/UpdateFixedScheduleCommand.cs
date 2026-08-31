using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.UpdateFixedSchedule;

public sealed record UpdateFixedScheduleCommand(
    Guid ScheduleId,
    SchoolDayOfWeek DayOfWeek,
    Guid PeriodId,
    Guid AssignmentId,
    Guid RoomId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;