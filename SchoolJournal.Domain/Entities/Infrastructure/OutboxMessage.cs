namespace SchoolJournal.Domain.Entities.Infrastructure;

public sealed record OutboxMessage
{
    public Guid Id { get; init; }

    public Guid CreatedByUserId { get; init; }

    public required string Type { get; init; }

    public required string Content { get; init; }

    public DateTimeOffset OccurredOnUtc { get; init; }

    public DateTimeOffset? ProcessedOnUtc { get; init; }

    public string? Error { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}