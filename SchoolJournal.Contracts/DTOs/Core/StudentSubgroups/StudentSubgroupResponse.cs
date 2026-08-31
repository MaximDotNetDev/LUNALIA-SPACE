namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record StudentSubgroupResponse(
    Guid StudentSubgroupId,
    Guid StudentId,
    Guid SubgroupId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);