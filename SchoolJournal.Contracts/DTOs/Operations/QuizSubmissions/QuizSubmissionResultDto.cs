namespace SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

public sealed record QuizSubmissionResultDto(
    Guid SubmissionId,
    string StudentFullName,
    int Score,
    int MaxScore,
    DateTime SubmittedAt
);