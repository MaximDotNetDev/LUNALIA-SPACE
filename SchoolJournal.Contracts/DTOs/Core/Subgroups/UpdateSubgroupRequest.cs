namespace SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record UpdateSubgroupRequest(
    string SubgroupName,
    bool IsActive,
    string RowVersionBase64
);