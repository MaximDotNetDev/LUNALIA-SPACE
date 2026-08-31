namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

public sealed record CreateQuizQuestionRequest(
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);