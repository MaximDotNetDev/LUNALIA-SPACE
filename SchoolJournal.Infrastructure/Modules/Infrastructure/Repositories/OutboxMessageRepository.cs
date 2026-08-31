using Dapper;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Infrastructure.Repositories;

public sealed class OutboxMessageRepository(SqlConnectionFactory connectionFactory) : IOutboxMessageRepository
{
    public async Task<Guid> AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        const string sql = """
            INSERT INTO [Infrastructure].[OutboxMessages] (
                CreatedByUserId,
                Type,
                Content,
                OccurredOnUtc
            )
            OUTPUT INSERTED.Id
            VALUES (
                @CreatedByUserId,
                @Type,
                @Content,
                @OccurredOnUtc
            );
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                message.CreatedByUserId,
                message.Type,
                message.Content,
                message.OccurredOnUtc
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, DateTimeOffset? processedOnUtc, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Infrastructure].[OutboxMessages]
            SET ProcessedOnUtc = @ProcessedOnUtc,
                Error = @Error,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id 
              AND IsDeleted = 0 
              AND ProcessedOnUtc IS NULL;
            """;

        using var connection = connectionFactory.CreateConnection();

        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id, ProcessedOnUtc = processedOnUtc, Error = errorMessage },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return rowsAffected > 0;
    }

    public async Task<int> DeleteProcessedOlderThanAsync(DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM [Infrastructure].[OutboxMessages]
            WHERE ProcessedOnUtc < @OlderThanUtc;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { OlderThanUtc = olderThanUtc },
cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (@BatchSize) *
            FROM [Infrastructure].[OutboxMessages]
            WHERE ProcessedOnUtc IS NULL 
              AND IsDeleted = 0
            ORDER BY OccurredOnUtc ASC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<OutboxMessage>(new CommandDefinition(
            sql,
            new { BatchSize = batchSize },
cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<OutboxMessage> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int take,
        string? type = null,
        bool? hasError = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Infrastructure].[OutboxMessages]
            WHERE (@Type IS NULL OR Type = @Type)
              AND (@HasError IS NULL OR (@HasError = 1 AND Error IS NOT NULL) OR (@HasError = 0 AND Error IS NULL));

            SELECT * FROM [Infrastructure].[OutboxMessages]
            WHERE (@Type IS NULL OR Type = @Type)
              AND (@HasError IS NULL OR (@HasError = 1 AND Error IS NOT NULL) OR (@HasError = 0 AND Error IS NULL))
            ORDER BY OccurredOnUtc DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take, Type = type, HasError = hasError },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<OutboxMessage>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Infrastructure].[OutboxMessages] WHERE Id = @Id;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<OutboxMessage>(new CommandDefinition(
            sql,
            new { Id = id },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}