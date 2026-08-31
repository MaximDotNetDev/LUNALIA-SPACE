namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions.Content;

public sealed record CrosswordContentDto(
    int Width,
    int Height,
    IReadOnlyCollection<CrosswordWordDto> Words,
    IReadOnlyCollection<CrosswordCellDto> Cells);

public sealed record CrosswordWordDto(
    int Number,
    string Answer,
    string Clue,
    int Row,
    int Column,
    bool IsHorizontal);

public sealed record CrosswordCellDto(
    int Row,
    int Column,
    char Letter,
    bool IsEmpty,
    string Number);