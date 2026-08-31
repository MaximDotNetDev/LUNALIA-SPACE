namespace SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

public sealed record SubmitQuizRequest(
    Guid AssignmentId,
    IReadOnlyCollection<QuizAnswerDto> Answers
);