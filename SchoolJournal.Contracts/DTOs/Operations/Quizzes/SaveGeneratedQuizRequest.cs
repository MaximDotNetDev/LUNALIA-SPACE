namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record SaveGeneratedQuizRequest(
    Guid TeacherId,
    Guid SubjectId,
    Guid ClassId,
    string Title,
    IReadOnlyCollection<SaveGeneratedQuizQuestionRequest> Questions
);

public sealed record SaveGeneratedQuizQuestionRequest(
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);