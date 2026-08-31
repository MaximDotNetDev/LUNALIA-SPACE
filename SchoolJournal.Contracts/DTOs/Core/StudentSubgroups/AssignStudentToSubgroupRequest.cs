namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record AssignStudentToSubgroupRequest(
    Guid StudentId,
    Guid SubgroupId
);