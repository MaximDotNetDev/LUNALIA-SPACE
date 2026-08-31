namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record StudentHistoryResponse(
    Guid StudentId,
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid ClassId,
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo);