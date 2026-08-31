using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Core.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class SubgroupRepository(SqlConnectionFactory connectionFactory) : ISubgroupRepository
{
    public async Task<Guid> AddAsync(Subgroup subgroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subgroup);

        const string sql = """
            INSERT INTO [Core].[Subgroups] (ClassId, SubjectId, SubgroupName)
            OUTPUT INSERTED.SubgroupId
            VALUES (@ClassId, @SubjectId, @SubgroupName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { subgroup.ClassId, subgroup.SubjectId, subgroup.SubgroupName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subgroup?> GetByIdAsync(Guid subgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Subgroups] WHERE SubgroupId = @SubgroupId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Subgroup>(new CommandDefinition(
            sql,
            new { SubgroupId = subgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameInClassAsync(Guid classId, string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subgroups] 
                WHERE ClassId = @ClassId AND SubgroupName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { ClassId = classId, Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ClassExistsAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Classes] WHERE ClassId = @ClassId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { ClassId = classId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subjects] WHERE SubjectId = @SubjectId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subgroup?> UpdateAsync(Subgroup subgroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subgroup);

        const string sql = """
            UPDATE [Core].[Subgroups]
            SET SubgroupName = @SubgroupName,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE SubgroupId = @SubgroupId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subgroup>(new CommandDefinition(
            sql,
            new
            {
                subgroup.SubgroupName,
                subgroup.IsActive,
                subgroup.UpdatedAt,
                subgroup.SubgroupId,
                RowVersion = subgroup.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameInClassExcludingIdAsync(Guid classId, string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subgroups] 
                WHERE ClassId = @ClassId AND SubgroupName = @Name AND SubgroupId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { ClassId = classId, Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subgroup?> DeleteAsync(Guid subgroupId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Subgroups]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE SubgroupId = @SubgroupId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subgroup>(new CommandDefinition(
            sql,
            new { SubgroupId = subgroupId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subgroup?> RestoreAsync(Guid subgroupId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Subgroups]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE SubgroupId = @SubgroupId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subgroup>(new CommandDefinition(
            sql,
            new { SubgroupId = subgroupId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Subgroup>> GetByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Subgroups] WHERE ClassId = @ClassId AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Subgroup>(new CommandDefinition(
            sql,
            new { ClassId = classId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Subgroup>> GetBySubjectAndClassIdAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Core].[Subgroups] 
            WHERE ClassId = @ClassId 
              AND SubjectId = @SubjectId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Subgroup>(new CommandDefinition(
            sql,
            new { ClassId = classId, SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<SubgroupListItem> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Core].[Subgroups] WHERE IsDeleted = 0;

            SELECT 
                sg.SubgroupId,
                sg.ClassId,
                c.ClassName,
                sg.SubjectId,
                sub.SubjectName,
                sg.SubgroupName,
                sg.IsActive,
                sg.RowVersion
            FROM [Core].[Subgroups] sg
            INNER JOIN [Core].[Classes] c ON sg.ClassId = c.ClassId
            INNER JOIN [Core].[Subjects] sub ON sg.SubjectId = sub.SubjectId
            WHERE sg.IsDeleted = 0
            ORDER BY sg.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<SubgroupListItem>().ConfigureAwait(false);

        return (items, totalCount);
    }
}