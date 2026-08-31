using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class QualificationRepository(SqlConnectionFactory connectionFactory) : IQualificationRepository
{
    public async Task<Guid> AddAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(qualification);

        const string sql = """
            INSERT INTO [Reference].[Qualifications] (QualificationName)
            OUTPUT INSERTED.QualificationId
            VALUES (@QualificationName);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { qualification.QualificationName },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Qualifications] 
                WHERE QualificationName = @Name AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Name = name },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Qualification?> GetByIdAsync(Guid qualificationId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[Qualifications] WHERE QualificationId = @QualificationId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Qualification>(new CommandDefinition(
            sql,
new { QualificationId = qualificationId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Qualification?> UpdateAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(qualification);

        const string sql = """
            UPDATE [Reference].[Qualifications]
            SET QualificationName = @QualificationName,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE QualificationId = @QualificationId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Qualification>(new CommandDefinition(
            sql,
            new
            {
                qualification.QualificationName,
                qualification.UpdatedAt,
                qualification.QualificationId,
                RowVersion = qualification.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Qualifications] 
                WHERE QualificationName = @Name AND QualificationId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
new { Name = name, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Qualification?> DeleteAsync(Guid qualificationId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Qualifications]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE QualificationId = @QualificationId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Qualification>(new CommandDefinition(
            sql,
new { QualificationId = qualificationId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Qualification?> RestoreAsync(Guid qualificationId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Qualifications]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE QualificationId = @QualificationId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Qualification>(new CommandDefinition(
            sql,
new { QualificationId = qualificationId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Qualification> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[Qualifications] WHERE IsDeleted = 0;

            SELECT * FROM [Reference].[Qualifications]
            WHERE IsDeleted = 0
            ORDER BY QualificationName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Qualification>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Qualification> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[Qualifications] WHERE IsDeleted = 1;

            SELECT * FROM [Reference].[Qualifications]
            WHERE IsDeleted = 1
            ORDER BY UpdatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Qualification>().ConfigureAwait(false);

        return (items, totalCount);
    }
}