using Dapper;
using SchoolJournal.Domain.Entities.Communications;
using SchoolJournal.Domain.Entities.Communications.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Communications.Repositories;

public sealed class AnnouncementRepository(SqlConnectionFactory connectionFactory) : IAnnouncementRepository
{
    public async Task<Guid> AddAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);

        const string sql = """
            INSERT INTO [Communications].[Announcements] 
                (Title, Content, AuthorId, ExpirationDate)
            OUTPUT INSERTED.AnnouncementId
            VALUES 
                (@Title, @Content, @AuthorId, @ExpirationDate);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                announcement.Title,
                announcement.Content,
                announcement.AuthorId,
                announcement.ExpirationDate
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Announcement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Communications].[Announcements] 
            WHERE AnnouncementId = @AnnouncementId;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Announcement>(new CommandDefinition(
            sql,
            new { AnnouncementId = announcementId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Announcement?> UpdateAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(announcement);

        const string sql = """
            UPDATE [Communications].[Announcements]
            SET Title = @Title,
                Content = @Content,
                ExpirationDate = @ExpirationDate,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AnnouncementId = @AnnouncementId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Announcement>(new CommandDefinition(
            sql,
            new
            {
                announcement.Title,
                announcement.Content,
                announcement.ExpirationDate,
                announcement.AnnouncementId,
                RowVersion = announcement.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Announcement?> ToggleStatusAsync(Guid announcementId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Communications].[Announcements]
            SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AnnouncementId = @AnnouncementId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Announcement>(new CommandDefinition(
            sql,
            new { AnnouncementId = announcementId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Announcement?> DeleteAsync(Guid announcementId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Communications].[Announcements]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AnnouncementId = @AnnouncementId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Announcement>(new CommandDefinition(
            sql,
            new { AnnouncementId = announcementId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Announcement> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Communications].[Announcements] 
            WHERE IsDeleted = 0 AND IsActive = 1 
              AND (ExpirationDate IS NULL OR ExpirationDate > GETUTCDATE());

            SELECT * FROM [Communications].[Announcements]
            WHERE IsDeleted = 0 AND IsActive = 1 
              AND (ExpirationDate IS NULL OR ExpirationDate > GETUTCDATE())
            ORDER BY DateCreated DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { Skip = skip, Take = take }, cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Announcement>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Announcement> Items, int TotalCount)> GetAdminPagedAsync(int skip, int take, string? searchTerm, bool? isActive, Guid? authorId, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Skip", skip);
        parameters.Add("Take", take);

        var conditions = new List<string> { "IsDeleted = 0" };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            conditions.Add("(Title LIKE @Search OR Content LIKE @Search)");
            parameters.Add("Search", $"%{searchTerm}%");
        }

        if (isActive.HasValue)
        {
            conditions.Add("IsActive = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        if (authorId.HasValue)
        {
            conditions.Add("AuthorId = @AuthorId");
            parameters.Add("AuthorId", authorId.Value);
        }

        var whereClause = string.Join(" AND ", conditions);

        var sql = $"""
            SELECT COUNT(*) FROM [Communications].[Announcements] WHERE {whereClause};

            SELECT * FROM [Communications].[Announcements] 
            WHERE {whereClause}
            ORDER BY CreatedAt DESC 
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Announcement>().ConfigureAwait(false);

        return (items, totalCount);
    }
}