namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

public sealed record QuizQuestionResponse(
    Guid QuestionId,
    Guid QuizId,
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64
);