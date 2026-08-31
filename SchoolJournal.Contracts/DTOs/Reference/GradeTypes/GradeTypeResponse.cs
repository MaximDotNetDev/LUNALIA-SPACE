namespace SchoolJournal.Contracts.DTOs.Reference.GradeTypes;

public sealed record GradeTypeResponse(
    Guid GradeTypeId,
    string TypeName,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);