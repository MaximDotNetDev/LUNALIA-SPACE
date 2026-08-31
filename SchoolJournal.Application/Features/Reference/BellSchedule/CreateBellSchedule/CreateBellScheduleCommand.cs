using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.CreateBellSchedule;

public sealed record CreateBellScheduleCommand(
    int LessonNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
) : IRequest<ErrorOr<Guid>>;