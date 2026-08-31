namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record StudentAttendanceItem(
    Guid StudentId,
    string Status,
    string? Comment
);

public sealed record BulkRecordAttendanceRequest(
    Guid LessonId,
    IEnumerable<StudentAttendanceItem> Students
);