using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;

namespace SchoolJournal.Application.Features.Infrastructure.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? UserId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null) : IRequest<IEnumerable<AuditLogResponse>>;