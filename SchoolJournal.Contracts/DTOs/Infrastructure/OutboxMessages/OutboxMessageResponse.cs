namespace SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

public sealed record OutboxMessageResponse(
    Guid Id,
    Guid CreatedByUserId,
    string Type,
    string Content,
    DateTimeOffset OccurredOnUtc,
    DateTimeOffset? ProcessedOnUtc,
    string? Error,
    DateTimeOffset CreatedAt);