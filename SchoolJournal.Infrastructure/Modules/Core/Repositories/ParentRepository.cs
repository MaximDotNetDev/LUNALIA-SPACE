using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class ParentRepository(SqlConnectionFactory connectionFactory) : IParentRepository
{
    public async Task<Guid> AddAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);

        const string sql = """
            INSERT INTO [Core].[Parents] (LastName, FirstName, MiddleName, Phone)
            OUTPUT INSERTED.ParentId
            VALUES (@LastName, @FirstName, @MiddleName, @Phone);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { parent.LastName, parent.FirstName, parent.MiddleName, parent.Phone },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Parent?> GetByIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Parents] WHERE ParentId = @ParentId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new { ParentId = parentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Parent?> UpdateAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);

        const string sql = """
            UPDATE [Core].[Parents]
            SET LastName = @LastName,
                FirstName = @FirstName,
                MiddleName = @MiddleName,
                Phone = @Phone,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE ParentId = @ParentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new
            {
                parent.LastName,
                parent.FirstName,
                parent.MiddleName,
                parent.Phone,
                parent.UpdatedAt,
                parent.ParentId,
                RowVersion = parent.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Parent?> DeleteAsync(Guid parentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Parents]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE ParentId = @ParentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new { ParentId = parentId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Parent?> LinkToUserAsync(Guid parentId, Guid userId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Parents]
            SET UserId = @UserId,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE ParentId = @ParentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new { ParentId = parentId, UserId = userId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Parent?> ToggleStatusAsync(Guid parentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Parents]
            SET IsActive = ~IsActive,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE ParentId = @ParentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new { ParentId = parentId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<SchoolJournal.Domain.Entities.Core.Models.ParentListItemResult> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Core].[Parents] WHERE IsDeleted = 0;

            SELECT 
                p.ParentId, p.LastName, p.FirstName, p.MiddleName, 
                p.Phone, p.UserId, p.IsActive, p.CreatedAt, p.UpdatedAt, p.RowVersion,
                u.Login
            FROM [Core].[Parents] p
            LEFT JOIN [Identity].[Users] u ON p.UserId = u.UserId
            WHERE p.IsDeleted = 0
            ORDER BY p.LastName ASC, p.FirstName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql, new { Skip = skip, Take = take }, cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<SchoolJournal.Domain.Entities.Core.Models.ParentListItemResult>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<Parent?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Parents] WHERE UserId = @UserId AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Parent>(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}