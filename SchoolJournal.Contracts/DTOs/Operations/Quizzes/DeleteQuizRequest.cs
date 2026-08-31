namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record DeleteQuizRequest(
    string RowVersionBase64
);