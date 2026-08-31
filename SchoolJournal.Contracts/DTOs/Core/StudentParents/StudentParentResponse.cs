namespace SchoolJournal.Contracts.DTOs.Core.StudentParents;

public sealed record StudentParentResponse(
    Guid StudentParentId,
    Guid StudentId,
    Guid ParentId,
    string? Role,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);