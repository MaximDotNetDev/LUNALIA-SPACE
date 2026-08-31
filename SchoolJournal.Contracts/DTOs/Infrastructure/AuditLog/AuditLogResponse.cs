namespace SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;

public sealed record AuditLogResponse(
    long AuditId,
    Guid? UserId,
    string? UserLogin,
    string EntityName,
    string EntityRef,
    string Action,
    string? OldValue,
    string? NewValue,
    DateTimeOffset OccurredAtUtc,
    string? ClientIp);