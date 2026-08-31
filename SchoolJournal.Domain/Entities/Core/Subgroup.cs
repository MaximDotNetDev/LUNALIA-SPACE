namespace SchoolJournal.Domain.Entities.Core;

public sealed record Subgroup
{
    public Guid SubgroupId { get; init; }

    public Guid ClassId { get; init; }

    public Guid SubjectId { get; init; }

    public required string SubgroupName { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}