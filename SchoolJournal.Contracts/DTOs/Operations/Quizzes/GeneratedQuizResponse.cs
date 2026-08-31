namespace SchoolJournal.Contracts.DTOs.Operations.Quizzes;

public sealed record GeneratedQuestionResponse(
    int OrderIndex,
    string QuestionText,
    int QuestionType,
    string ContentJson,
    int Points
);

public sealed record GeneratedQuizResponse(
    string Title,
    IReadOnlyCollection<GeneratedQuestionResponse> Questions
);