namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record StudentInSubgroupResponse(
    Guid StudentSubgroupId,
    Guid StudentId,
    string FirstName,
    string LastName,
    string? Patronymic
);