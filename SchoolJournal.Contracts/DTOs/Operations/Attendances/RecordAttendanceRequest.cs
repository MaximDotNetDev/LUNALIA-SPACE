namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record RecordAttendanceRequest(
    Guid LessonId,
    Guid StudentId,
    string Status,
    string? Comment
);