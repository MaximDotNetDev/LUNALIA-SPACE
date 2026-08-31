namespace SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

public sealed record MarkOutboxMessageFailedRequest(
    string ErrorMessage,
    bool StopRetrying = false
);