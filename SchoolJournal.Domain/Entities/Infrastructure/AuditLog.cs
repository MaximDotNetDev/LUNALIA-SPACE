namespace SchoolJournal.Domain.Entities.Infrastructure;

public sealed record AuditLog
{
    public long AuditId { get; init; }

    public Guid? UserId { get; init; }

    public required string EntityName { get; init; }

    public required string EntityRef { get; init; }

    public required string Action { get; init; }

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public string? ClientIp { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public string? UserLogin { get; init; }
}