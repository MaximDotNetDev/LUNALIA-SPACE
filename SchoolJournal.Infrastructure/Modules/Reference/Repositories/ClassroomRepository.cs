using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class ClassroomRepository(SqlConnectionFactory connectionFactory) : IClassroomRepository
{
    public async Task<Guid> AddAsync(Classroom classroom, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(classroom);

        const string sql = """
            INSERT INTO [Reference].[Classrooms] (RoomNumber, Name, Capacity)
            OUTPUT INSERTED.RoomId
            VALUES (@RoomNumber, @Name, @Capacity);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { classroom.RoomNumber, classroom.Name, classroom.Capacity },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByRoomNumberAsync(string roomNumber, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Classrooms] 
                WHERE RoomNumber = @RoomNumber AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { RoomNumber = roomNumber },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Classroom?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[Classrooms] WHERE RoomId = @RoomId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Classroom>(new CommandDefinition(
            sql,
            new { RoomId = roomId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Classroom?> UpdateAsync(Classroom classroom, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(classroom);

        const string sql = """
            UPDATE [Reference].[Classrooms]
            SET RoomNumber = @RoomNumber,
                Name = @Name,
                Capacity = @Capacity,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE RoomId = @RoomId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Classroom>(new CommandDefinition(
            sql,
            new
            {
                classroom.RoomNumber,
                classroom.Name,
                classroom.Capacity,
                classroom.RoomId,
                RowVersion = classroom.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByRoomNumberExcludingIdAsync(string roomNumber, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[Classrooms] 
                WHERE RoomNumber = @RoomNumber AND RoomId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { RoomNumber = roomNumber, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Classroom?> DeleteAsync(Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Classrooms]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE RoomId = @RoomId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Classroom>(new CommandDefinition(
            sql,
            new { RoomId = roomId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Classroom?> RestoreAsync(Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Reference].[Classrooms]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE RoomId = @RoomId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Classroom>(new CommandDefinition(
            sql,
            new { RoomId = roomId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Classroom> Items, int TotalCount)> GetActivePagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(false, searchTerm, skip, take, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Classroom> Items, int TotalCount)> GetDeletedPagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetPagedInternalAsync(true, searchTerm, skip, take, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(IEnumerable<Classroom> Items, int TotalCount)> GetPagedInternalAsync(bool isDeleted, string? searchTerm, int skip, int take, CancellationToken cancellationToken)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
        var searchPattern = hasSearch ? $"%{searchTerm}%" : null;

        const string sql = """
            SELECT COUNT(*) FROM [Reference].[Classrooms] 
            WHERE IsDeleted = @IsDeleted 
              AND (@HasSearch = 0 OR RoomNumber LIKE @SearchTerm OR Name LIKE @SearchTerm);

            SELECT * FROM [Reference].[Classrooms]
            WHERE IsDeleted = @IsDeleted
              AND (@HasSearch = 0 OR RoomNumber LIKE @SearchTerm OR Name LIKE @SearchTerm)
            ORDER BY RoomNumber ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { IsDeleted = isDeleted, HasSearch = hasSearch, SearchTerm = searchPattern, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Classroom>().ConfigureAwait(false);

        return (items, totalCount);
    }
}