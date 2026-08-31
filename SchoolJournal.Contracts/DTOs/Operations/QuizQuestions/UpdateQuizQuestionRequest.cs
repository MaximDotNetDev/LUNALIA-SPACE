namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

public sealed record UpdateQuizQuestionRequest(
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points,
    string RowVersionBase64
);