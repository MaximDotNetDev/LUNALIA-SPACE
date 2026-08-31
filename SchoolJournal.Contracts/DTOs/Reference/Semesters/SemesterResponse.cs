namespace SchoolJournal.Contracts.DTOs.Reference.Semesters;

public sealed record SemesterResponse(
    Guid SemesterId,
    string SemesterName,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string RowVersionBase64
);