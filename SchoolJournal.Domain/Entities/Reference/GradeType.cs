namespace SchoolJournal.Domain.Entities.Reference;

public sealed record GradeType
{
    public Guid GradeTypeId { get; init; }

    public required string TypeName { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}