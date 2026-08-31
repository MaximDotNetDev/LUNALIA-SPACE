using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Entities.Operations.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class FixedScheduleRepository(SqlConnectionFactory connectionFactory) : IFixedScheduleRepository
{
    public async Task<Guid> AddAsync(FixedSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        const string sql = """
            INSERT INTO [Operations].[FixedSchedules] (DayOfWeek, PeriodId, AssignmentId, RoomId)
            OUTPUT INSERTED.ScheduleId
            VALUES (@DayOfWeek, @PeriodId, @AssignmentId, @RoomId);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                DayOfWeek = (int)schedule.DayOfWeek,
                schedule.PeriodId,
                schedule.AssignmentId,
                schedule.RoomId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    private const string ReadModelSql = """
        SELECT 
            fs.ScheduleId, fs.DayOfWeek, fs.PeriodId, fs.AssignmentId, fs.RoomId, fs.IsDeleted, fs.CreatedAt, fs.UpdatedAt, fs.RowVersion,
            CONCAT(bs.LessonNumber, ' урок (', FORMAT(bs.StartTime, 'HH:mm'), ' - ', FORMAT(bs.EndTime, 'HH:mm'), ')') AS PeriodName,
            CONCAT('Каб. ', cr.RoomNumber, ISNULL(' - ' + cr.Name, '')) AS RoomName,
            CONCAT(sb.SubjectName, ' — ', cl.ClassName, ISNULL(' (' + sg.SubgroupName + ')', ''), ' — ', t.LastName, ' ', SUBSTRING(t.FirstName, 1, 1), '.', ISNULL(SUBSTRING(t.MiddleName, 1, 1) + '.', '')) AS AssignmentDetails
        FROM [Operations].[FixedSchedules] fs
        INNER JOIN [Reference].[BellSchedules] bs ON fs.PeriodId = bs.ScheduleId
        INNER JOIN [Reference].[Classrooms] cr ON fs.RoomId = cr.RoomId
        INNER JOIN [Operations].[TeachingAssignments] ta ON fs.AssignmentId = ta.AssignmentId
        INNER JOIN [Core].[Subjects] sb ON ta.SubjectId = sb.SubjectId
        INNER JOIN [Core].[Classes] cl ON ta.ClassId = cl.ClassId
        INNER JOIN [Core].[Teachers] t ON ta.TeacherId = t.TeacherId
        LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
        """;

    public async Task<FixedScheduleReadModel?> GetByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        const string sql = ReadModelSql + " WHERE fs.ScheduleId = @ScheduleId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<FixedScheduleReadModel>(new CommandDefinition(
            sql,
            new { ScheduleId = scheduleId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
    public async Task<bool> HasOverlappingRoomAsync(int dayOfWeek, Guid periodId, Guid roomId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[FixedSchedules]
                WHERE DayOfWeek = @DayOfWeek 
                  AND PeriodId = @PeriodId 
                  AND RoomId = @RoomId 
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { DayOfWeek = dayOfWeek, PeriodId = periodId, RoomId = roomId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingAssignmentAsync(int dayOfWeek, Guid periodId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[FixedSchedules]
                WHERE DayOfWeek = @DayOfWeek 
                  AND PeriodId = @PeriodId 
                  AND AssignmentId = @AssignmentId 
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { DayOfWeek = dayOfWeek, PeriodId = periodId, AssignmentId = assignmentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<FixedSchedule?> UpdateAsync(FixedSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        const string sql = """
            UPDATE [Operations].[FixedSchedules]
            SET DayOfWeek = @DayOfWeek,
                PeriodId = @PeriodId,
                AssignmentId = @AssignmentId,
                RoomId = @RoomId,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE ScheduleId = @ScheduleId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<FixedSchedule>(new CommandDefinition(
            sql,
            new
            {
                DayOfWeek = (int)schedule.DayOfWeek,
                schedule.PeriodId,
                schedule.AssignmentId,
                schedule.RoomId,
                schedule.UpdatedAt,
                schedule.ScheduleId,
                RowVersion = schedule.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingRoomExcludingIdAsync(int dayOfWeek, Guid periodId, Guid roomId, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[FixedSchedules]
                WHERE DayOfWeek = @DayOfWeek 
                  AND PeriodId = @PeriodId 
                  AND RoomId = @RoomId 
                  AND ScheduleId != @ExcludeId
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { DayOfWeek = dayOfWeek, PeriodId = periodId, RoomId = roomId, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingAssignmentExcludingIdAsync(int dayOfWeek, Guid periodId, Guid assignmentId, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[FixedSchedules]
                WHERE DayOfWeek = @DayOfWeek 
                  AND PeriodId = @PeriodId 
                  AND AssignmentId = @AssignmentId 
                  AND ScheduleId != @ExcludeId
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { DayOfWeek = dayOfWeek, PeriodId = periodId, AssignmentId = assignmentId, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<FixedSchedule?> DeleteAsync(Guid scheduleId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[FixedSchedules]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE ScheduleId = @ScheduleId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<FixedSchedule>(new CommandDefinition(
            sql,
            new { ScheduleId = scheduleId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<FixedScheduleReadModel>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = ReadModelSql + " WHERE fs.AssignmentId = @AssignmentId AND fs.IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<FixedScheduleReadModel>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<FixedScheduleReadModel>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        const string sql = ReadModelSql + " WHERE fs.RoomId = @RoomId AND fs.IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<FixedScheduleReadModel>(new CommandDefinition(
            sql,
            new { RoomId = roomId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<FixedScheduleReadModel>> GetByDayAsync(int dayOfWeek, CancellationToken cancellationToken = default)
    {
        const string sql = ReadModelSql + " WHERE fs.DayOfWeek = @DayOfWeek AND fs.IsDeleted = 0 ORDER BY bs.StartTime ASC;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<FixedScheduleReadModel>(new CommandDefinition(
            sql,
            new { DayOfWeek = dayOfWeek },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}