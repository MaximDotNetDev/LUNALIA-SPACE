namespace SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record CreateSubgroupRequest(
    Guid ClassId,
    Guid SubjectId,
    string SubgroupName
);