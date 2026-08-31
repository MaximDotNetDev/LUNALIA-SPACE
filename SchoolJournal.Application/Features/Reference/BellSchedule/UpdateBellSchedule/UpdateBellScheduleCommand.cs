using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.UpdateBellSchedule;

public sealed record UpdateBellScheduleCommand(
    Guid ScheduleId,
    int LessonNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
) : IRequest<ErrorOr<Success>>;