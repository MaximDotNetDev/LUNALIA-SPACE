namespace SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

public sealed record PedagogicalTitleResponse(
    Guid TitleId,
    string TitleName,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);