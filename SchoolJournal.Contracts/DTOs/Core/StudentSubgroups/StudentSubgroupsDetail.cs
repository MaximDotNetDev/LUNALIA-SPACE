namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record StudentSubgroupsDetail(
    Guid StudentId,
    IEnumerable<SubgroupItemModel> Subgroups
);

public sealed record SubgroupItemModel(
    Guid StudentSubgroupId,
    Guid SubgroupId,
    string SubgroupName,
    Guid ClassId,
    Guid SubjectId
);