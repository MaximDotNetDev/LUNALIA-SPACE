namespace SchoolJournal.Contracts.DTOs.Operations.Grades;

public sealed record GradeResponse(
    Guid GradeId,
    Guid LessonId,
    Guid StudentId,
    string GradeValue,
    string? Comment,
    Guid CreatedByUserId,
    Guid UpdatedByUserId,
    Guid GradeTypeId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64
);