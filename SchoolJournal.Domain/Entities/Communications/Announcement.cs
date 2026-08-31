namespace SchoolJournal.Domain.Entities.Communications;

public sealed record Announcement
{
    public Guid AnnouncementId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public Guid AuthorId { get; init; }

    public DateTimeOffset DateCreated { get; init; }

    public DateTimeOffset? ExpirationDate { get; init; }

    public bool IsActive { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}