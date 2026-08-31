using Dapper;
using System.Linq;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class SemesterRepository(SqlConnectionFactory connectionFactory) : ISemesterRepository
{
    public async Task<Guid> AddAsync(Semester semester, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semester);

        const string sql = """
            INSERT INTO [Reference].[Semesters] (SemesterName, StartDate, EndDate)
            OUTPUT INSERTED.SemesterId
            VALUES (@SemesterName, @StartDate, @EndDate);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { semester.SemesterName, semester.StartDate, semester.EndDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Semesters] 
                WHERE SemesterName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingDatesAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Semesters]
                WHERE IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    sql,
                    new { StartDate = startDate, EndDate = endDate },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Semester?> UpdateAsync(Semester semester, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(semester);

        const string sql = """
            UPDATE [Reference].[Semesters]
            SET SemesterName = @SemesterName,
                StartDate = @StartDate,
                EndDate = @EndDate,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE SemesterId = @SemesterId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Semester>(new CommandDefinition(
                    sql,
                    new
                    {
                        semester.SemesterName,
                        semester.StartDate,
                        semester.EndDate,
                        semester.UpdatedAt,
                        semester.SemesterId,
                        RowVersion = semester.RowVersion.ToArray()
                    },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Semesters] 
                WHERE SemesterName = @Name AND SemesterId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingDatesExcludingIdAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Semesters]
                WHERE IsDeleted = 0 AND SemesterId != @ExcludeId
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    sql,
                    new { StartDate = startDate, EndDate = endDate, ExcludeId = excludeId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Semester?> DeleteAsync(Guid semesterId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Semesters]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE SemesterId = @SemesterId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Semester>(new CommandDefinition(
            sql,
            new { SemesterId = semesterId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Semester?> GetByIdAsync(Guid semesterId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[Semesters] WHERE SemesterId = @SemesterId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Semester>(new CommandDefinition(
            sql,
            new { SemesterId = semesterId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Semester?> RestoreAsync(Guid semesterId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Semesters]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE SemesterId = @SemesterId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Semester>(new CommandDefinition(
            sql,
            new { SemesterId = semesterId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Semester> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(false, skip, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Semester> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(true, skip, take, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<Semester> Items, int TotalCount)> GetPagedInternalAsync(bool isDeleted, int skip, int take, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[Semesters] WHERE IsDeleted = @IsDeleted;

            SELECT * FROM [Reference].[Semesters]
            WHERE IsDeleted = @IsDeleted
            ORDER BY StartDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { IsDeleted = isDeleted, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Semester>().ConfigureAwait(false);

        return (items, totalCount);
    }
}