namespace SchoolJournal.Contracts.DTOs.Operations.Lessons;

public sealed record RescheduleLessonRequest(
    DateTimeOffset LessonDate,
    Guid PeriodId,
    Guid RoomId,
    string RowVersionBase64
);