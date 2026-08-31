using Dapper;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Infrastructure.Repositories;

public sealed class AuditLogRepository(SqlConnectionFactory connectionFactory) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [Infrastructure].[AuditLogs] 
                (UserId, EntityName, EntityRef, Action, OldValue, NewValue, OccurredAtUtc, ClientIp, CreatedAt)
            VALUES 
                (NULLIF(@UserId, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)), @EntityName, @EntityRef, @Action, @OldValue, @NewValue, @OccurredAtUtc, @ClientIp, @CreatedAt);
            """;

        using var connection = connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            auditLog,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<AuditLog>> GetByFilterAsync(Guid? userId, DateTimeOffset? fromDate, DateTimeOffset? toDate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                a.*, 
                u.Login AS UserLogin 
            FROM [Infrastructure].[AuditLogs] a
            LEFT JOIN [Identity].[Users] u ON a.UserId = u.UserId
            WHERE (@UserId IS NULL OR a.UserId = @UserId)
              AND (@From IS NULL OR a.OccurredAtUtc >= @From)
              AND (@To IS NULL OR a.OccurredAtUtc <= @To)
            ORDER BY a.OccurredAtUtc DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<AuditLog>(new CommandDefinition(
            sql,
            new { UserId = userId, From = fromDate, To = toDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}