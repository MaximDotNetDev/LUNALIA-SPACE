namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record QuizDetailResponse(
    Guid QuizId,
    Guid TeacherId,
    Guid SubjectId,
    string Title,
    string RowVersionBase64,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyCollection<QuizQuestionResponse> Questions
);