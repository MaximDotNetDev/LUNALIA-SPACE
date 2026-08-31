using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class PedagogicalTitleRepository(SqlConnectionFactory connectionFactory) : IPedagogicalTitleRepository
{
    public async Task<Guid> AddAsync(PedagogicalTitle pedagogicalTitle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedagogicalTitle);

        const string sql = """
            INSERT INTO [Reference].[PedagogicalTitles] (TitleName)
            OUTPUT INSERTED.TitleId
            VALUES (@TitleName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { pedagogicalTitle.TitleName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[PedagogicalTitles] 
                WHERE TitleName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<PedagogicalTitle?> UpdateAsync(PedagogicalTitle pedagogicalTitle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedagogicalTitle);

        const string sql = """
            UPDATE [Reference].[PedagogicalTitles]
            SET TitleName = @TitleName,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE TitleId = @TitleId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<PedagogicalTitle>(new CommandDefinition(
            sql,
            new
            {
                pedagogicalTitle.TitleName,
                pedagogicalTitle.UpdatedAt,
                pedagogicalTitle.TitleId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[PedagogicalTitles] 
                WHERE TitleName = @Name AND TitleId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<PedagogicalTitle?> DeleteAsync(Guid titleId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[PedagogicalTitles]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE TitleId = @TitleId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<PedagogicalTitle>(new CommandDefinition(
            sql,
            new { TitleId = titleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<PedagogicalTitle?> RestoreAsync(Guid titleId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[PedagogicalTitles]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE TitleId = @TitleId 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<PedagogicalTitle>(new CommandDefinition(
            sql,
            new { TitleId = titleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<PedagogicalTitle>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Reference].[PedagogicalTitles] 
            WHERE IsDeleted = 0 
            ORDER BY TitleName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<PedagogicalTitle>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<PedagogicalTitle> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(false, skip, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<PedagogicalTitle> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(true, skip, take, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<PedagogicalTitle> Items, int TotalCount)> GetPagedInternalAsync(bool isDeleted, int skip, int take, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[PedagogicalTitles] WHERE IsDeleted = @IsDeleted;

            SELECT * FROM [Reference].[PedagogicalTitles]
            WHERE IsDeleted = @IsDeleted
            ORDER BY TitleName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { IsDeleted = isDeleted, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<PedagogicalTitle>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<PedagogicalTitle?> GetByIdAsync(Guid titleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[PedagogicalTitles] WHERE TitleId = @TitleId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<PedagogicalTitle>(new CommandDefinition(
            sql,
            new { TitleId = titleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}