namespace SchoolJournal.Domain.Entities.Core;

public sealed record Parent
{
    public Guid ParentId { get; init; }

    public string? LastName { get; init; }

    public string? FirstName { get; init; }

    public string? MiddleName { get; init; }

    public string? Phone { get; init; }

    public Guid? UserId { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}