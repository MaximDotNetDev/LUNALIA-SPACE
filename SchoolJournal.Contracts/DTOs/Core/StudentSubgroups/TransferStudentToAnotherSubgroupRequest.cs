namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record TransferStudentToAnotherSubgroupRequest(
    Guid NewSubgroupId
);