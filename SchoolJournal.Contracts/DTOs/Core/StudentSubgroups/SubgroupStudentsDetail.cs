namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record SubgroupStudentsDetail(
    Guid SubgroupId,
    IEnumerable<SubgroupStudentModel> Students
);

public sealed record SubgroupStudentModel(
    Guid StudentSubgroupId,
    Guid StudentId,
    string FirstName,
    string LastName,
    string? Patronymic
);