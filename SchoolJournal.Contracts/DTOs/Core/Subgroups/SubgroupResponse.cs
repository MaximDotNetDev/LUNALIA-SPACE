namespace SchoolJournal.Contracts.DTOs.Core.Subgroups;

public sealed record SubgroupResponse(
    Guid SubgroupId,
    Guid ClassId,
    string ClassName,
    Guid SubjectId,
    string SubjectName,
    string SubgroupName,
    bool IsActive,
    string RowVersionBase64
);