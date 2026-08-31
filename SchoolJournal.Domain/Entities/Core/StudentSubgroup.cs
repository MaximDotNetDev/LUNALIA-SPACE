namespace SchoolJournal.Domain.Entities.Core;

public sealed record StudentSubgroup
{
    public Guid StudentSubgroupId { get; init; }

    public Guid StudentId { get; init; }

    public Guid SubgroupId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}