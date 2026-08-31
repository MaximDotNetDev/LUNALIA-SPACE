namespace SchoolJournal.Contracts.DTOs.Core.Students;

public sealed record StudentSearchResponse(
    Guid StudentId,
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid ClassId,
    bool IsActive,
    DateTimeOffset CreatedAt, 
    Guid? UserId,  
    string? Login 
);