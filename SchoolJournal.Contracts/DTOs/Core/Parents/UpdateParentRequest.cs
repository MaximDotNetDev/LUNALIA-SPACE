namespace SchoolJournal.Contracts.DTOs.Core.Parents;

public sealed record UpdateParentRequest(
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string? Phone,
    string RowVersionBase64
);