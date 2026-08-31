namespace SchoolJournal.Contracts.DTOs.Reference.Semesters;

public sealed record UpdateSemesterRequest(
    string SemesterName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
);