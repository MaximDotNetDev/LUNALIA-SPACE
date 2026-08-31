namespace SchoolJournal.Contracts.DTOs.Operations.Grades;

public sealed record UpdateGradeRequest(
    string GradeValue,
    string? Comment,
    Guid GradeTypeId,
    string RowVersionBase64
);