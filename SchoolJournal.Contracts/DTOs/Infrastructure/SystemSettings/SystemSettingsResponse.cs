namespace SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;

public sealed record SystemSettingsResponse(
    string SchoolName,
    string AcademicYear,
    string? PrincipalName,
    string RowVersionBase64
);