using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Contracts.DTOs.Core.StudentParents;

public sealed record StudentParentDetailResponse(
    Guid StudentParentId,
    string? Role,
    ParentResponse Parent
);