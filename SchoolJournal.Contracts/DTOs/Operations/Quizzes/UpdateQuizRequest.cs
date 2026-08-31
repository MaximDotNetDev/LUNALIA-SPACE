namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record UpdateQuizQuestionRequest(
    Guid? QuestionId,
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);

public sealed record UpdateQuizRequest(
    Guid SubjectId,
    string Title,
    string RowVersionBase64,
    IReadOnlyCollection<UpdateQuizQuestionRequest> Questions
);