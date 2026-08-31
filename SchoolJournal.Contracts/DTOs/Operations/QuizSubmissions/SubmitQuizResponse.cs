namespace SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

public sealed record SubmitQuizResponse(
    Guid SubmissionId,
    int Score,
    int MaxScore,
    DateTime SubmittedAt,
    int EarnedCoins
);