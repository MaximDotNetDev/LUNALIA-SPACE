namespace SchoolJournal.Contracts.DTOs.Core.Parents;

public sealed record ParentResponse(
    Guid ParentId,
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string? Phone,
    Guid? UserId,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64,
    string? Login
);