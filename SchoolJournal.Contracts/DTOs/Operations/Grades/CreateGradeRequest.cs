namespace SchoolJournal.Contracts.DTOs.Operations.Grades;

public sealed record CreateGradeRequest(
    Guid LessonId,
    Guid StudentId,
    string GradeValue,
    string? Comment,
    Guid GradeTypeId
);