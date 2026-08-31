using Refit;
using SchoolJournal.Contracts.DTOs.Infrastructure.AuditLog;

namespace SchoolJournal.Client.Core.Features.Infrastructure.Logs;

public interface IAuditLogsApi
{
    [Get("/api/audit-logs")]
    public Task<IEnumerable<AuditLogResponse>> GetAuditLogsAsync(
        [Query] Guid? userId = null,
        [Query] DateTimeOffset? fromDate = null,
        [Query] DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);
}