namespace SchoolJournal.Contracts.DTOs.Reference.Semesters;

public sealed record CreateSemesterRequest(
    string SemesterName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate
);