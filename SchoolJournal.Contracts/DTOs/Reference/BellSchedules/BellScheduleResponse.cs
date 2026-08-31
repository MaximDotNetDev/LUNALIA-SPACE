namespace SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

public sealed record BellScheduleResponse(
    Guid ScheduleId,
    int LessonNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);