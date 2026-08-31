using Dapper;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Reference.Repositories;

public sealed class BellScheduleRepository(SqlConnectionFactory connectionFactory) : IBellScheduleRepository
{
    public async Task<Guid> AddAsync(BellSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        const string sql = """
            INSERT INTO [Reference].[BellSchedules] (LessonNumber, StartTime, EndTime)
            OUTPUT INSERTED.ScheduleId
            VALUES (@LessonNumber, @StartTime, @EndTime);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { schedule.LessonNumber, schedule.StartTime, schedule.EndTime },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByLessonNumberAsync(int lessonNumber, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[BellSchedules] 
                WHERE LessonNumber = @LessonNumber AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { LessonNumber = lessonNumber },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingTimeAsync(DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[BellSchedules]
                WHERE IsDeleted = 0
                  AND (@StartTime < EndTime AND @EndTime > StartTime)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { StartTime = startTime, EndTime = endTime },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<BellSchedule?> GetByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Reference].[BellSchedules] WHERE ScheduleId = @ScheduleId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<BellSchedule>(new CommandDefinition(
            sql,
            new { ScheduleId = scheduleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<BellSchedule?> UpdateAsync(BellSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        const string sql = """
            UPDATE [Reference].[BellSchedules]
            SET LessonNumber = @LessonNumber,
                StartTime = @StartTime,
                EndTime = @EndTime,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ScheduleId = @ScheduleId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<BellSchedule>(new CommandDefinition(
            sql,
            new
            {
                schedule.LessonNumber,
                schedule.StartTime,
                schedule.EndTime,
                schedule.ScheduleId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByLessonNumberExcludingIdAsync(int lessonNumber, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[BellSchedules] 
                WHERE LessonNumber = @LessonNumber AND ScheduleId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { LessonNumber = lessonNumber, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingTimeExcludingIdAsync(DateTimeOffset startTime, DateTimeOffset endTime, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Reference].[BellSchedules]
                WHERE IsDeleted = 0 AND ScheduleId != @ExcludeId
                  AND (@StartTime < EndTime AND @EndTime > StartTime)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { StartTime = startTime, EndTime = endTime, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<BellSchedule?> DeleteAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Reference].[BellSchedules]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ScheduleId = @ScheduleId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<BellSchedule>(new CommandDefinition(
            sql,
            new { ScheduleId = scheduleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<BellSchedule> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Reference].[BellSchedules] WHERE IsDeleted = 0;

            SELECT * FROM [Reference].[BellSchedules]
            WHERE IsDeleted = 0
            ORDER BY LessonNumber ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<BellSchedule>().ConfigureAwait(false);

        return (items, totalCount);
    }
}