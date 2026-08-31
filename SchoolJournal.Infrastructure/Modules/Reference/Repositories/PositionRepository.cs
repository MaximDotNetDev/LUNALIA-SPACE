using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class PositionRepository(SqlConnectionFactory connectionFactory) : IPositionRepository
{
    public async Task<bool> ExistsByNameAsync(string positionName, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Positions] 
                WHERE PositionName = @PositionName AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { PositionName = positionName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Guid> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(position);

        const string sql = """
            INSERT INTO [Reference].[Positions] (PositionName)
            OUTPUT INSERTED.PositionId
            VALUES (@PositionName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { position.PositionName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Position?> GetByIdAsync(Guid positionId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[Positions] WHERE PositionId = @PositionId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Position>(new CommandDefinition(
            sql,
            new { PositionId = positionId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string positionName, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Positions] 
                WHERE PositionName = @PositionName AND PositionId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { PositionName = positionName, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Position?> UpdateAsync(Position position, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(position);

        const string sql = """
            UPDATE [Reference].[Positions]
            SET PositionName = @PositionName,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE PositionId = @PositionId AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Position>(new CommandDefinition(
                    sql,
                    new { position.PositionName, position.PositionId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Position?> DeleteAsync(Guid positionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[Positions]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE PositionId = @PositionId AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Position>(new CommandDefinition(
                    sql,
                    new { PositionId = positionId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Position> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[Positions] WHERE IsDeleted = 0;

            SELECT * FROM [Reference].[Positions]
            WHERE IsDeleted = 0
            ORDER BY PositionName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Position>().ConfigureAwait(false);

        return (items, totalCount);
    }
}