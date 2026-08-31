namespace SchoolJournal.Domain.Entities.Operations;

public sealed record QuizAssignment
{
    public Guid AssignmentId { get; init; }

    public Guid QuizId { get; init; }

    public Guid ClassId { get; init; }

    public DateTimeOffset AssignedDate { get; init; }

    public DateTimeOffset? DueDate { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}