using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsQuery, IEnumerable<AuditLogResponse>>
{
    public async Task<IEnumerable<AuditLogResponse>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var logs = await auditLogRepository.GetByFilterAsync(request.UserId, request.FromDate, request.ToDate, cancellationToken).ConfigureAwait(false);

        return logs.Select(l => new AuditLogResponse(
            l.AuditId,
            l.UserId,
            l.UserLogin,
            l.EntityName,
            l.EntityRef,
            l.Action,
            l.OldValue,
            l.NewValue,
            l.OccurredAtUtc,
            l.ClientIp));
    }
}