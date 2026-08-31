namespace SchoolJournal.Contracts.DTOs.AiGeneration;

public sealed record CrosswordAiResponseDto(
    string Title,
    IReadOnlyCollection<CrosswordAiWordDto> Words);

public sealed record CrosswordAiWordDto(
    string Answer,
    string Clue);
