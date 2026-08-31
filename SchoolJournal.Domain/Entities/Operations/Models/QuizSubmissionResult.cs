namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record QuizSubmissionResult
{
    public Guid SubmissionId { get; init; }
    public string StudentFullName { get; init; } = string.Empty;
    public int Score { get; init; }
    public int MaxScore { get; init; }
    public DateTimeOffset SubmittedAt { get; init; }
}