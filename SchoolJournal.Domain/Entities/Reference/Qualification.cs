namespace SchoolJournal.Domain.Entities.Reference;

public sealed record Qualification
{
    public Guid QualificationId { get; init; }

    public required string QualificationName { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}