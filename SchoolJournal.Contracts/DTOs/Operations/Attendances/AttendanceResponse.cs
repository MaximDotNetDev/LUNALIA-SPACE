namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record AttendanceResponse(
    Guid AttendanceId,
    Guid LessonId,
    Guid StudentId,
    string Status,
    string? Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64
);