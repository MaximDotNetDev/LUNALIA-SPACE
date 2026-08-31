namespace SchoolJournal.Contracts.DTOs.Core.Teachers;

public sealed record ToggleTeacherStatusRequest(
    bool IsActive,
    string RowVersionBase64
);