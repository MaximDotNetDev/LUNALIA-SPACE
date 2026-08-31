namespace SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;

public sealed record AvailableStudentModel(
    Guid StudentId,
    string FirstName,
    string LastName,
    string? Patronymic
);