namespace SchoolJournal.Domain.Entities.Core;

public sealed record Subject
{
    public Guid SubjectId { get; init; }

    public required string SubjectName { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}