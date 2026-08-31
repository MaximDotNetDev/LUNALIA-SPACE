namespace SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

public sealed record CreateBellScheduleRequest(
    int LessonNumber,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);