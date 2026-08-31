namespace SchoolJournal.Contracts.DTOs.Core.Parents;

public sealed record CreateParentRequest(
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string? Phone
);