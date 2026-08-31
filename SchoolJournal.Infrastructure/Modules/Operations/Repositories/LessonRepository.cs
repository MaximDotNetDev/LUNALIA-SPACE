using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;
using System.Linq;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class LessonRepository(SqlConnectionFactory connectionFactory) : ILessonRepository
{
    public async Task<Guid> AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        const string sql = """
            INSERT INTO [Operations].[Lessons] (
                AssignmentId, LessonDate, LessonTopic, Homework, 
                LessonTypeId, PeriodId, RoomId, SemesterId
            )
            OUTPUT INSERTED.LessonId
            VALUES (
                @AssignmentId, @LessonDate, @LessonTopic, @Homework, 
                @LessonTypeId, @PeriodId, @RoomId, @SemesterId
            );
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                lesson.AssignmentId,
                lesson.LessonDate,
                lesson.LessonTopic,
                lesson.Homework,
                lesson.LessonTypeId,
                lesson.PeriodId,
                lesson.RoomId,
                lesson.SemesterId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Lesson?> GetByIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Lessons] WHERE LessonId = @LessonId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Lesson>(new CommandDefinition(
            sql,
            new { LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> VerifyAssignmentOwnershipAsync(Guid assignmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [Operations].[TeachingAssignments] ta
                INNER JOIN [Core].[Teachers] t ON ta.TeacherId = t.TeacherId
                WHERE ta.AssignmentId = @AssignmentId
                  AND t.UserId = @UserId
                  AND ta.IsActive = 1
                  AND ta.IsDeleted = 0
                  AND t.IsActive = 1
                  AND t.IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    sql,
                    new { AssignmentId = assignmentId, UserId = userId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> VerifyLessonOwnershipAsync(Guid lessonId, Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM [Operations].[Lessons] l
                INNER JOIN [Operations].[TeachingAssignments] ta ON l.AssignmentId = ta.AssignmentId
                INNER JOIN [Core].[Teachers] t ON ta.TeacherId = t.TeacherId
                WHERE l.LessonId = @LessonId
                  AND t.UserId = @UserId
                  AND l.IsDeleted = 0
                  AND ta.IsActive = 1
                  AND ta.IsDeleted = 0
                  AND t.IsActive = 1
                  AND t.IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { LessonId = lessonId, UserId = userId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Lesson?> UpdateTopicAndHomeworkAsync(Guid lessonId, string? lessonTopic, string? homework, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Lessons]
            SET LessonTopic = @LessonTopic,
                Homework = @Homework,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE LessonId = @LessonId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Lesson>(new CommandDefinition(
            sql,
            new
            {
                LessonId = lessonId,
                LessonTopic = lessonTopic,
                Homework = homework,
                RowVersion = rowVersion
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Lesson?> RescheduleAsync(Guid lessonId, DateTimeOffset lessonDate, Guid periodId, Guid roomId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Lessons]
            SET LessonDate = @LessonDate,
                PeriodId = @PeriodId,
                RoomId = @RoomId,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE LessonId = @LessonId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Lesson>(new CommandDefinition(
            sql,
            new
            {
                LessonId = lessonId,
                LessonDate = lessonDate,
                PeriodId = periodId,
                RoomId = roomId,
                RowVersion = rowVersion
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Lesson?> DeleteAsync(Guid lessonId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Lessons]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE LessonId = @LessonId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Lesson>(new CommandDefinition(
                    sql,
                    new { LessonId = lessonId, RowVersion = rowVersion },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Guid> GetSubjectIdByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ta.SubjectId 
            FROM [Operations].[Lessons] l
            INNER JOIN [Operations].[TeachingAssignments] ta ON l.AssignmentId = ta.AssignmentId
            WHERE l.LessonId = @LessonId AND l.IsDeleted = 0;
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql, new { LessonId = lessonId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}