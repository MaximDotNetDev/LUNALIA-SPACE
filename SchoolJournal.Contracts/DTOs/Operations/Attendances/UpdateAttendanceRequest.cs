namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record UpdateAttendanceRequest(
    string Status,
    string? Comment,
    string RowVersionBase64
);