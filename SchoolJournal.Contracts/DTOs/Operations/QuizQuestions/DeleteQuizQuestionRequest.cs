namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

public sealed record DeleteQuizQuestionRequest(
    string RowVersionBase64
);