namespace SchoolJournal.Domain.Entities.Operations;

public sealed record QuizSubmission
{
    public Guid SubmissionId { get; init; }

    public Guid AssignmentId { get; init; }

    public Guid StudentId { get; init; }

    public int Score { get; init; }

    public int MaxScore { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}