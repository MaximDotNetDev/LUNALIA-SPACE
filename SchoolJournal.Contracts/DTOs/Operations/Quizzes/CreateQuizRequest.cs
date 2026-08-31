namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record CreateQuizRequest(
    Guid TeacherId,
    Guid SubjectId,
    string Title
);