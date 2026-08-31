namespace SchoolJournal.Contracts.DTOs.Operations.Grades;

public sealed record DeleteGradeRequest(
    string RowVersionBase64
);