namespace SchoolJournal.Contracts.DTOs.Reference.Positions;

public sealed record PositionResponse(
    Guid PositionId,
    string PositionName
);