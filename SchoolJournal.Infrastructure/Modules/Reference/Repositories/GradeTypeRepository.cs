using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class GradeTypeRepository(SqlConnectionFactory connectionFactory) : IGradeTypeRepository
{
    public async Task<Guid> AddAsync(GradeType gradeType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gradeType);

        const string sql = """
            INSERT INTO [Reference].[GradeTypes] (TypeName)
            OUTPUT INSERTED.GradeTypeId
            VALUES (@TypeName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { gradeType.TypeName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[GradeTypes] 
                WHERE TypeName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<GradeType?> GetByIdAsync(Guid gradeTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[GradeTypes] WHERE GradeTypeId = @GradeTypeId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<GradeType>(new CommandDefinition(
            sql,
            new { GradeTypeId = gradeTypeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[GradeTypes] 
                WHERE TypeName = @Name AND GradeTypeId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<GradeType?> UpdateAsync(GradeType gradeType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gradeType);

        const string sql = """
            UPDATE [Reference].[GradeTypes]
            SET TypeName = @TypeName,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE GradeTypeId = @GradeTypeId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<GradeType>(new CommandDefinition(
            sql,
            new
            {
                gradeType.TypeName,
                gradeType.UpdatedAt,
                gradeType.GradeTypeId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
    public async Task<GradeType?> DeleteAsync(Guid gradeTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[GradeTypes]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE GradeTypeId = @GradeTypeId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<GradeType>(new CommandDefinition(
                    sql,
                    new { GradeTypeId = gradeTypeId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<GradeType?> RestoreAsync(Guid gradeTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[GradeTypes]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE GradeTypeId = @GradeTypeId 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<GradeType>(new CommandDefinition(
            sql,
            new { GradeTypeId = gradeTypeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<GradeType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[GradeTypes] WHERE IsDeleted = 0 ORDER BY TypeName;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<GradeType>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<GradeType> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[GradeTypes] WHERE IsDeleted = 1;

            SELECT * FROM [Reference].[GradeTypes]
            WHERE IsDeleted = 1
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<GradeType>().ConfigureAwait(false);

        return (items, totalCount);
    }
}