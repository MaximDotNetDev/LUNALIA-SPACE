namespace SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;

public sealed record UpdateSystemSettingsRequest(
    string SchoolName,
    string AcademicYear,
    string? PrincipalName,
    string? RowVersionBase64
);