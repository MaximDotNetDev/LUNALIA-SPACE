using Dapper;
using Microsoft.Data.SqlClient;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class AttendanceRepository(SqlConnectionFactory connectionFactory) : IAttendanceRepository
{
    public async Task<Guid> AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendance);

        const string sql = """
            INSERT INTO [Operations].[Attendances] (AttendanceId, LessonId, StudentId, Status, Comment, IsDeleted, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.AttendanceId
            VALUES (@AttendanceId, @LessonId, @StudentId, @Status, @Comment, @IsDeleted, @CreatedAt, @UpdatedAt);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                attendance.AttendanceId,
                attendance.LessonId,
                attendance.StudentId,
                attendance.Status,
                attendance.Comment,
                attendance.IsDeleted,
                attendance.CreatedAt,
                attendance.UpdatedAt
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByLessonAndStudentAsync(Guid lessonId, Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[Attendances] 
                WHERE LessonId = @LessonId AND StudentId = @StudentId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { LessonId = lessonId, StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> LessonExistsAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[Lessons] WHERE LessonId = @LessonId
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> StudentExistsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] WHERE StudentId = @StudentId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsTeacherAssignedToLessonAsync(Guid teacherId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[Lessons] L
                LEFT JOIN [Operations].[TeacherSubstitutions] TS ON L.LessonId = TS.LessonId AND TS.IsDeleted = 0
                WHERE L.LessonId = @LessonId 
                  AND (L.TeacherId = @TeacherId OR TS.SubstituteTeacherId = @TeacherId)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Attendance?> GetByIdAsync(Guid attendanceId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Attendances] WHERE AttendanceId = @AttendanceId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Attendance>(new CommandDefinition(
            sql,
            new { AttendanceId = attendanceId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Attendance?> UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendance);

        const string sql = """
            UPDATE [Operations].[Attendances]
            SET Status = @Status,
                Comment = @Comment,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE AttendanceId = @AttendanceId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Attendance>(new CommandDefinition(
                    sql,
                    new
                    {
                        attendance.Status,
                        attendance.Comment,
                        attendance.UpdatedAt,
                        attendance.AttendanceId,
                        RowVersion = attendance.RowVersion.ToArray()
                    },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Attendance>> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Attendances] WHERE LessonId = @LessonId AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Attendance>(new CommandDefinition(
            sql,
            new { LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task BulkUpsertAsync(Guid lessonId, IEnumerable<Attendance> attendances, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attendances);

        const string sql = """
            MERGE [Operations].[Attendances] AS Target
            USING (
                SELECT 
                    @LessonId AS LessonId,
                    CAST(AttendanceId AS UNIQUEIDENTIFIER) AS AttendanceId,
                    CAST(StudentId AS UNIQUEIDENTIFIER) AS StudentId,
                    Status,
                    Comment
                FROM OPENJSON(@JsonData)
                WITH (
                    AttendanceId UNIQUEIDENTIFIER '$.AttendanceId',
                    StudentId UNIQUEIDENTIFIER '$.StudentId',
                    Status NVARCHAR(20) '$.Status',
                    Comment NVARCHAR(255) '$.Comment'
                )
            ) AS Source
            ON Target.LessonId = Source.LessonId AND Target.StudentId = Source.StudentId AND Target.IsDeleted = 0
            WHEN MATCHED THEN
                UPDATE SET 
                    Target.Status = Source.Status,
                    Target.Comment = Source.Comment,
                    Target.UpdatedAt = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (AttendanceId, LessonId, StudentId, Status, Comment, IsDeleted, CreatedAt, UpdatedAt)
                VALUES (Source.AttendanceId, Source.LessonId, Source.StudentId, Source.Status, Source.Comment, 0, GETUTCDATE(), NULL);
            """;

        var jsonData = System.Text.Json.JsonSerializer.Serialize(attendances.Select(a => new
        {
            a.AttendanceId,
            a.StudentId,
            a.Status,
            a.Comment
        }));

        using var connection = connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { LessonId = lessonId, JsonData = jsonData },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Attendance?> SoftDeleteAsync(Guid attendanceId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Attendances]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE AttendanceId = @AttendanceId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Attendance>(new CommandDefinition(
                    sql,
                    new { AttendanceId = attendanceId, RowVersion = rowVersion },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsStudentUserAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] 
                WHERE StudentId = @StudentId AND UserId = @UserId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UserId = userId, StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsParentOfStudentUserAsync(Guid userId, Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[StudentParents] SP
                INNER JOIN [Core].[Parents] P ON SP.ParentId = P.ParentId
                WHERE SP.StudentId = @StudentId AND P.UserId = @UserId AND P.IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UserId = userId, StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolJournal.Domain.Entities.Operations.Models.LessonRegisterData?> GetLessonRegisterInternalAsync(System.Guid lessonId, System.Threading.CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT LessonId, LessonTopic, LessonDate 
            FROM [Operations].[Lessons] 
            WHERE LessonId = @LessonId AND IsDeleted = 0;

            SELECT 
                S.StudentId,
                S.LastName,
                S.FirstName,
                S.MiddleName,
                A.AttendanceId,
                A.Status,
                A.Comment,
                A.RowVersion
            FROM [Operations].[Lessons] L
            INNER JOIN [Operations].[TeachingAssignments] TA ON L.AssignmentId = TA.AssignmentId
            INNER JOIN [Core].[Students] S ON TA.ClassId = S.ClassId AND S.IsDeleted = 0 AND S.IsActive = 1
            LEFT JOIN [Operations].[Attendances] A ON L.LessonId = A.LessonId AND S.StudentId = A.StudentId AND A.IsDeleted = 0
            WHERE L.LessonId = @LessonId AND L.IsDeleted = 0
            ORDER BY S.LastName ASC, S.FirstName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var lessonHeader = await multi.ReadFirstOrDefaultAsync<dynamic>().ConfigureAwait(false);
        if (lessonHeader is null)
        {
            return null;
        }

        var rows = await multi.ReadAsync<SchoolJournal.Domain.Entities.Operations.Models.LessonRegisterRow>().ConfigureAwait(false);

        return new SchoolJournal.Domain.Entities.Operations.Models.LessonRegisterData
        {
            LessonId = (System.Guid)lessonHeader.LessonId,
            LessonTopic = (string?)lessonHeader.LessonTopic,
            LessonDate = (System.DateTimeOffset)lessonHeader.LessonDate,
            Rows = rows
        };
    }

    public async Task<System.Collections.Generic.IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceHistoryRow>> GetStudentHistoryInternalAsync(System.Guid studentId, System.DateTimeOffset? startDate, System.DateTimeOffset? endDate, System.Threading.CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                L.LessonDate,
                L.LessonTopic,
                S.SubjectName,
                A.AttendanceId,
                A.Status,
                A.Comment,
                A.RowVersion
            FROM [Operations].[Attendances] A
            INNER JOIN [Operations].[Lessons] L ON A.LessonId = L.LessonId AND L.IsDeleted = 0
            INNER JOIN [Operations].[TeachingAssignments] TA ON L.AssignmentId = TA.AssignmentId AND TA.IsDeleted = 0
            INNER JOIN [Reference].[Subjects] S ON TA.SubjectId = S.SubjectId
            WHERE A.StudentId = @StudentId AND A.IsDeleted = 0
              AND (@StartDate IS NULL OR L.LessonDate >= @StartDate)
              AND (@EndDate IS NULL OR L.LessonDate <= @EndDate)
            ORDER BY L.LessonDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceHistoryRow>(new CommandDefinition(
            sql,
            new { StudentId = studentId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsTeacherAssignedToStudentsClassAsync(System.Guid teacherId, System.Guid studentId, System.Threading.CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] ST
                INNER JOIN [Operations].[TeachingAssignments] TA ON ST.ClassId = TA.ClassId AND TA.IsDeleted = 0
                LEFT JOIN [Operations].[TeacherSubstitutions] TS ON TA.AssignmentId = TS.AssignmentId AND TS.IsDeleted = 0
                WHERE ST.StudentId = @StudentId 
                  AND ST.IsDeleted = 0 
                  AND (TA.TeacherId = @TeacherId OR TS.SubstituteTeacherId = @TeacherId)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceStatsData> GetStudentStatsInternalAsync(System.Guid studentId, System.DateTimeOffset? startDate, System.DateTimeOffset? endDate, System.Threading.CancellationToken cancellationToken = default)
    {
        const string sql = """
            -- Загальна агрегована статистика учня
            SELECT 
                COUNT(A.AttendanceId) AS TotalLessons,
                SUM(CASE WHEN A.Status = 'Present' THEN 1 ELSE 0 END) AS TotalPresent,
                SUM(CASE WHEN A.Status = 'Absent' THEN 1 ELSE 0 END) AS TotalAbsent,
                SUM(CASE WHEN A.Status = 'Late' THEN 1 ELSE 0 END) AS TotalLate
            FROM [Operations].[Attendances] A
            INNER JOIN [Operations].[Lessons] L ON A.LessonId = L.LessonId AND L.IsDeleted = 0
            WHERE A.StudentId = @StudentId AND A.IsDeleted = 0
              AND (@StartDate IS NULL OR L.LessonDate >= @StartDate)
              AND (@EndDate IS NULL OR L.LessonDate <= @EndDate);

            -- Розріз статистики по навчальних предметах
            SELECT 
                S.SubjectName,
                COUNT(A.AttendanceId) AS TotalLessons,
                SUM(CASE WHEN A.Status = 'Present' THEN 1 ELSE 0 END) AS PresentCount,
                SUM(CASE WHEN A.Status = 'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
                SUM(CASE WHEN A.Status = 'Late' THEN 1 ELSE 0 END) AS LateCount
            FROM [Operations].[Attendances] A
            INNER JOIN [Operations].[Lessons] L ON A.LessonId = L.LessonId AND L.IsDeleted = 0
            INNER JOIN [Operations].[TeachingAssignments] TA ON L.AssignmentId = TA.AssignmentId AND TA.IsDeleted = 0
            INNER JOIN [Reference].[Subjects] S ON TA.SubjectId = S.SubjectId
            WHERE A.StudentId = @StudentId AND A.IsDeleted = 0
              AND (@StartDate IS NULL OR L.LessonDate >= @StartDate)
              AND (@EndDate IS NULL OR L.LessonDate <= @EndDate)
            GROUP BY S.SubjectName
            ORDER BY S.SubjectName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { StudentId = studentId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var overall = await multi.ReadSingleOrDefaultAsync<dynamic>().ConfigureAwait(false);
        var subjects = await multi.ReadAsync<SchoolJournal.Domain.Entities.Operations.Models.SubjectAttendanceStats>().ConfigureAwait(false);

        if (overall is null)
        {
            return new SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceStatsData
            {
                TotalLessons = 0,
                TotalPresent = 0,
                TotalAbsent = 0,
                TotalLate = 0,
                Subjects = []
            };
        }

        return new SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceStatsData
        {
            TotalLessons = overall.TotalLessons ?? 0,
            TotalPresent = overall.TotalPresent ?? 0,
            TotalAbsent = overall.TotalAbsent ?? 0,
            TotalLate = overall.TotalLate ?? 0,
            Subjects = subjects
        };
    }
}