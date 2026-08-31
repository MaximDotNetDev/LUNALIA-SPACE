namespace SchoolJournal.Contracts.DTOs.Reference.LessonTypes;

public sealed record LessonTypeResponse(
    Guid LessonTypeId,
    string TypeName,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);