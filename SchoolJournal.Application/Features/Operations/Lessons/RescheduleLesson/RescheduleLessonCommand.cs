using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Lessons.RescheduleLesson;

public sealed record RescheduleLessonCommand(
    Guid LessonId,
    DateTimeOffset LessonDate,
    Guid PeriodId,
    Guid RoomId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;