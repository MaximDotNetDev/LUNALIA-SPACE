using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class SubjectRepository(SqlConnectionFactory connectionFactory) : ISubjectRepository
{
    public async Task<Guid> AddAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subject);

        const string sql = """
            INSERT INTO [Core].[Subjects] (SubjectName)
            OUTPUT INSERTED.SubjectId
            VALUES (@SubjectName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { subject.SubjectName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subjects] 
                WHERE SubjectName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subject?> GetByIdAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Subjects] WHERE SubjectId = @SubjectId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Subject>(new CommandDefinition(
            sql,
            new { SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subject?> UpdateAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subject);

        const string sql = """
            UPDATE [Core].[Subjects]
            SET SubjectName = @SubjectName,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE SubjectId = @SubjectId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subject>(new CommandDefinition(
            sql,
            new
            {
                subject.SubjectName,
                subject.UpdatedAt,
                subject.SubjectId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subjects] 
                WHERE SubjectName = @Name AND SubjectId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subject?> DeleteAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Core].[Subjects]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE SubjectId = @SubjectId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subject>(new CommandDefinition(
            sql,
            new { SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Subject?> RestoreAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Core].[Subjects]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE SubjectId = @SubjectId 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Subject>(new CommandDefinition(
            sql,
            new { SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Subject> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(false, skip, take, searchTerm, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Subject> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(true, skip, take, searchTerm, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<Subject> Items, int TotalCount)> GetPagedInternalAsync(bool isDeleted, int skip, int take, string? searchTerm, CancellationToken cancellationToken)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
        var searchPattern = hasSearch ? $"%{searchTerm!.Trim()}%" : null;

        const string sql = """
            SELECT COUNT(*) 
            FROM [Core].[Subjects] 
            WHERE IsDeleted = @IsDeleted 
              AND (@HasSearch = 0 OR SubjectName LIKE @SearchPattern);

            SELECT * FROM [Core].[Subjects]
            WHERE IsDeleted = @IsDeleted 
              AND (@HasSearch = 0 OR SubjectName LIKE @SearchPattern)
            ORDER BY SubjectName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new
            {
                IsDeleted = isDeleted,
                Skip = skip,
                Take = take,
                HasSearch = hasSearch,
                SearchPattern = searchPattern
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Subject>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT DISTINCT s.* FROM [Core].[Subjects] s
        INNER JOIN [Operations].[TeachingAssignments] ta ON s.SubjectId = ta.SubjectId
        WHERE ta.TeacherId = @TeacherId 
          AND ta.IsActive = 1 
          AND ta.IsDeleted = 0 
          AND s.IsDeleted = 0";

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<Subject>(new CommandDefinition(sql, new { TeacherId = teacherId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}