namespace SchoolJournal.Contracts.DTOs.Operations.QuizQuestions.Content;

public sealed record FillwordContentDto(
    int Width,
    int Height,
    IReadOnlyCollection<FillwordWordDto> Words,
    IReadOnlyCollection<FillwordCellDto> Cells
);

public sealed record FillwordWordDto(
    int Number,
    string Answer,
    string Clue,
    IReadOnlyCollection<FillwordCoordinateDto> Path
);

public sealed record FillwordCoordinateDto(int Row, int Column);

public sealed record FillwordCellDto(
    int Row,
    int Column,
    char Letter,
    bool IsRandom
);