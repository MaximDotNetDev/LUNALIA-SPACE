namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record QuizResponse(
    Guid QuizId,
    Guid TeacherId,
    Guid SubjectId,
    string Title,
    string RowVersionBase64,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);