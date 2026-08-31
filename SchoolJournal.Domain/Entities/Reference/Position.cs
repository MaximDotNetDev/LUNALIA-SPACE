namespace SchoolJournal.Domain.Entities.Reference;

public sealed record Position
{
    public Guid PositionId { get; init; }

    public required string PositionName { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}