namespace SchoolJournal.Domain.Entities.Operations;

public sealed record Quiz
{
    public Guid QuizId { get; init; }

    public Guid TeacherId { get; init; }

    public Guid SubjectId { get; init; }

    public required string Title { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}