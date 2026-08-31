namespace SchoolJournal.Contracts.DTOs.AiGeneration;

public sealed record FillwordAiResponseDto(
    string Title,
    IReadOnlyCollection<FillwordAiWordDto> Words
);

public sealed record FillwordAiWordDto(
    string Answer,
    string Clue
);