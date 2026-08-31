namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record StudentLookupResponse(
    Guid StudentId,
    string LastName,
    string FirstName,
    string? MiddleName,
    string Gender,
    bool IsActive);