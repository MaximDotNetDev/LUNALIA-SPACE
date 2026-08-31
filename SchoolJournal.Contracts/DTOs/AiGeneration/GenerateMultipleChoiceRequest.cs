namespace SchoolJournal.Contracts.DTOs.AiGeneration;

public sealed record GenerateMultipleChoiceRequest(
    string Text,
    int TotalQuestions = 12,
    int MultiAnswerCount = 0,
    int JudgmentCount = 0,
    int PointsPerQuestion = 1
);