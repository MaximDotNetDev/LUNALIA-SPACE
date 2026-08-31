namespace SchoolJournal.Domain.Entities.Core;

public sealed record StudentParent
{
    public Guid StudentParentId { get; init; }

    public Guid StudentId { get; init; }

    public Guid ParentId { get; init; }

    public string? Role { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}