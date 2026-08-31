namespace SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

public sealed record UpdateBellScheduleRequest(
    int LessonNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);