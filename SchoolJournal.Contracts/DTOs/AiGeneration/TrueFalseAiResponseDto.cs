namespace SchoolJournal.Contracts.DTOs.AiGeneration;

public sealed record TrueFalseAiResponseDto(
    string Title,
    IReadOnlyCollection<TrueFalseAiQuestionDto> Questions);

public sealed record TrueFalseAiQuestionDto(
    string Statement,
    bool IsTrue,
    string Explanation);