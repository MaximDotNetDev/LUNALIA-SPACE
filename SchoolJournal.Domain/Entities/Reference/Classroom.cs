namespace SchoolJournal.Domain.Entities.Reference;

public sealed record Classroom
{
    public Guid RoomId { get; init; }

    public required string RoomNumber { get; init; }

    public string? Name { get; init; }

    public int Capacity { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}