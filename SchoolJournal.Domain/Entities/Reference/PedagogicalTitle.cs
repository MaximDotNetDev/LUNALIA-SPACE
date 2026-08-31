namespace SchoolJournal.Domain.Entities.Reference;

public sealed record PedagogicalTitle
{
    public Guid TitleId { get; init; }

    public required string TitleName { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}