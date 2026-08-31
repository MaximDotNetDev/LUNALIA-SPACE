namespace SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

public sealed record QuizAnswerDto(
    Guid QuestionId,
    string AnswerJson
);