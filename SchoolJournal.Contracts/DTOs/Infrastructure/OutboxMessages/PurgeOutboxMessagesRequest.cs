namespace SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

public sealed record PurgeOutboxMessagesRequest(int OlderThanDays = 30);