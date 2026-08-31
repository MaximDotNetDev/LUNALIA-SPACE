namespace SchoolJournal.Domain.Entities.Operations;

public sealed record QuizQuestion
{
    public Guid QuestionId { get; init; }

    public Guid QuizId { get; init; }

    public int OrderIndex { get; init; }

    public required string QuestionText { get; init; }

    public int QuestionType { get; init; }

    public required string ContentJson { get; init; }

    public int Points { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}