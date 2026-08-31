namespace SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

public interface IAuditLogRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    public Task<IEnumerable<AuditLog>> GetByFilterAsync(Guid? userId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken cancellationToken = default);
}