namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record QuizQuestionResponse(
    Guid QuestionId,
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points,
    string RowVersionBase64
);