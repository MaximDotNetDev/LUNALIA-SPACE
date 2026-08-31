using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class LessonTypeRepository(SqlConnectionFactory connectionFactory) : ILessonTypeRepository
{
    public async Task<Guid> AddAsync(LessonType lessonType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lessonType);

        const string sql = """
            INSERT INTO [Reference].[LessonTypes] (TypeName)
            OUTPUT INSERTED.LessonTypeId
            VALUES (@TypeName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { lessonType.TypeName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string typeName, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[LessonTypes] 
                WHERE TypeName = @TypeName AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TypeName = typeName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<LessonType?> GetByIdAsync(Guid lessonTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[LessonTypes] WHERE LessonTypeId = @LessonTypeId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<LessonType>(new CommandDefinition(
            sql,
            new { LessonTypeId = lessonTypeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string typeName, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[LessonTypes] 
                WHERE TypeName = @TypeName AND LessonTypeId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TypeName = typeName, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<LessonType?> UpdateAsync(LessonType lessonType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lessonType);

        const string sql = """
            UPDATE [Reference].[LessonTypes]
            SET TypeName = @TypeName,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE LessonTypeId = @LessonTypeId AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<LessonType>(new CommandDefinition(
            sql,
            new
            {
                lessonType.TypeName,
                lessonType.UpdatedAt,
                lessonType.LessonTypeId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<LessonType?> DeleteAsync(Guid lessonTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[LessonTypes]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE LessonTypeId = @LessonTypeId AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<LessonType>(new CommandDefinition(
            sql,
            new { LessonTypeId = lessonTypeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<LessonType?> RestoreAsync(Guid lessonTypeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[LessonTypes]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE LessonTypeId = @LessonTypeId AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<LessonType>(new CommandDefinition(
            sql,
            new { LessonTypeId = lessonTypeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<LessonType> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(false, skip, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<LessonType> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(true, skip, take, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<LessonType> Items, int TotalCount)> GetPagedInternalAsync(bool isDeleted, int skip, int take, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[LessonTypes] WHERE IsDeleted = @IsDeleted;

            SELECT * FROM [Reference].[LessonTypes]
            WHERE IsDeleted = @IsDeleted
            ORDER BY TypeName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { IsDeleted = isDeleted, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<LessonType>().ConfigureAwait(false);

        return (items, totalCount);
    }
}