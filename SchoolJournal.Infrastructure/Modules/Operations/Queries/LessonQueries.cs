using Dapper;
using SchoolJournal.Application.Features.Operations.Lessons;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Queries;

public sealed class LessonQueries(SqlConnectionFactory connectionFactory) : ILessonQueries
{
    public async Task<bool> CheckReadAccessAsync(Guid lessonId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[Lessons] l
                INNER JOIN [Operations].[TeachingAssignments] ta ON l.AssignmentId = ta.AssignmentId
                WHERE l.LessonId = @LessonId AND l.IsDeleted = 0
                AND (
                    (@Role = 'Teacher' AND EXISTS (
                        SELECT 1 FROM [Core].[Teachers] t 
                        WHERE t.TeacherId = ta.TeacherId AND t.UserId = @UserId AND t.IsDeleted = 0
                    ))
                    OR
                    (@Role = 'Student' AND EXISTS (
                        SELECT 1 FROM [Core].[Students] s
                        WHERE s.UserId = @UserId AND s.ClassId = ta.ClassId 
                        AND (ta.SubgroupId IS NULL OR EXISTS (
                            SELECT 1 FROM [Core].[StudentSubgroups] ss 
                            WHERE ss.StudentId = s.StudentId AND ss.SubgroupId = ta.SubgroupId
                        ))
                    ))
                    OR
                    (@Role = 'Parent' AND EXISTS (
                        SELECT 1 FROM [Core].[Parents] p
                        INNER JOIN [Core].[StudentParents] sp ON p.ParentId = sp.ParentId
                        INNER JOIN [Core].[Students] s ON sp.StudentId = s.StudentId
                        WHERE p.UserId = @UserId AND s.ClassId = ta.ClassId
                        AND (ta.SubgroupId IS NULL OR EXISTS (
                            SELECT 1 FROM [Core].[StudentSubgroups] ss 
                            WHERE ss.SubgroupId = ta.SubgroupId AND ss.StudentId = s.StudentId
                        ))
                    ))
                )
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { LessonId = lessonId, UserId = userId, Role = role }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<LessonResponse?> GetDetailedByIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                l.LessonId, l.AssignmentId, l.LessonDate, l.LessonTopic, l.Homework, l.LessonTypeId,
                lt.TypeName AS LessonTypeName,
                l.PeriodId,
                CAST(bs.LessonNumber AS NVARCHAR(10)) AS PeriodName,
                l.RoomId,
                r.RoomNumber AS RoomName,
                l.SemesterId,
                l.RowVersion
            FROM [Operations].[Lessons] l
            INNER JOIN [Reference].[LessonTypes] lt ON l.LessonTypeId = lt.LessonTypeId
            INNER JOIN [Reference].[BellSchedules] bs ON l.PeriodId = bs.ScheduleId
            INNER JOIN [Reference].[Classrooms] r ON l.RoomId = r.RoomId
            WHERE l.LessonId = @LessonId AND l.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();
        var rawResult = await connection.QuerySingleOrDefaultAsync<dynamic>(new CommandDefinition(
            sql, new { LessonId = lessonId }, cancellationToken: cancellationToken)).ConfigureAwait(false);

        if (rawResult is null) return null;

        return new LessonResponse(
                    rawResult.LessonId, rawResult.AssignmentId, rawResult.LessonDate, rawResult.LessonTopic, rawResult.Homework,
                    rawResult.LessonTypeId, rawResult.LessonTypeName, rawResult.PeriodId, rawResult.PeriodName,
                    rawResult.RoomId, rawResult.RoomName, rawResult.SemesterId,
                    Convert.ToBase64String((byte[])rawResult.RowVersion)
                );
    }

    public async Task<bool> CheckAssignmentReadAccessAsync(Guid assignmentId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeachingAssignments] ta
                WHERE ta.AssignmentId = @AssignmentId AND ta.IsActive = 1 AND ta.IsDeleted = 0
                AND (
                    -- Вчитель
                    (@Role = 'Teacher' AND EXISTS (
                        SELECT 1 FROM [Core].[Teachers] t 
                        WHERE t.TeacherId = ta.TeacherId AND t.UserId = @UserId AND t.IsActive = 1 AND t.IsDeleted = 0
                    ))
                    OR
                    -- Учень
                    (@Role = 'Student' AND EXISTS (
                        SELECT 1 FROM [Core].[Students] s
                        WHERE s.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                        AND (ta.SubgroupId IS NULL OR EXISTS (
                            SELECT 1 FROM [Core].[StudentSubgroups] ss 
                            WHERE ss.StudentId = s.StudentId AND ss.SubgroupId = ta.SubgroupId
                        ))
                    ))
                    OR
                    -- Батько
                    (@Role = 'Parent' AND EXISTS (
                        SELECT 1 FROM [Core].[Parents] p
                        INNER JOIN [Core].[StudentParents] sp ON p.ParentId = sp.ParentId
                        INNER JOIN [Core].[Students] s ON sp.StudentId = s.StudentId
                        WHERE p.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                        AND (ta.SubgroupId IS NULL OR EXISTS (
                            SELECT 1 FROM [Core].[StudentSubgroups] ss 
                            WHERE ss.SubgroupId = ta.SubgroupId AND ss.StudentId = s.StudentId
                        ))
                    ))
                )
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { AssignmentId = assignmentId, UserId = userId, Role = role }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<LessonResponse>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                l.LessonId, l.AssignmentId, l.LessonDate, l.LessonTopic, l.Homework, l.LessonTypeId,
                lt.TypeName AS LessonTypeName,
                l.PeriodId, CAST(bs.LessonNumber AS NVARCHAR(10)) AS PeriodName,
                l.RoomId, r.RoomNumber AS RoomName,
                l.SemesterId, l.RowVersion
            FROM [Operations].[Lessons] l
            INNER JOIN [Reference].[LessonTypes] lt ON l.LessonTypeId = lt.LessonTypeId
            INNER JOIN [Reference].[BellSchedules] bs ON l.PeriodId = bs.ScheduleId
            INNER JOIN [Reference].[Classrooms] r ON l.RoomId = r.RoomId
            WHERE l.AssignmentId = @AssignmentId AND l.IsDeleted = 0
            ORDER BY l.LessonDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var rawResults = await connection.QueryAsync<dynamic>(new CommandDefinition(
            sql, new { AssignmentId = assignmentId }, cancellationToken: cancellationToken)).ConfigureAwait(false);

        return rawResults.Select(rawResult => new LessonResponse(
                    rawResult.LessonId, rawResult.AssignmentId, rawResult.LessonDate, rawResult.LessonTopic, rawResult.Homework,
                    rawResult.LessonTypeId, rawResult.LessonTypeName, rawResult.PeriodId, rawResult.PeriodName,
                    rawResult.RoomId, rawResult.RoomName, rawResult.SemesterId,
                    Convert.ToBase64String((byte[])rawResult.RowVersion)
                ));
    }

    public async Task<IEnumerable<LessonResponse>> GetScheduleAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid semesterId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                l.LessonId, l.AssignmentId, l.LessonDate, l.LessonTopic, l.Homework, l.LessonTypeId,
                lt.TypeName AS LessonTypeName,
                l.PeriodId, CAST(bs.LessonNumber AS NVARCHAR(10)) AS PeriodName,
                l.RoomId, r.RoomNumber AS RoomName,
                l.SemesterId, l.RowVersion
            FROM [Operations].[Lessons] l
            INNER JOIN [Operations].[TeachingAssignments] ta ON l.AssignmentId = ta.AssignmentId
            INNER JOIN [Reference].[LessonTypes] lt ON l.LessonTypeId = lt.LessonTypeId
            INNER JOIN [Reference].[BellSchedules] bs ON l.PeriodId = bs.ScheduleId
            INNER JOIN [Reference].[Classrooms] r ON l.RoomId = r.RoomId
            WHERE l.IsDeleted = 0 
              AND l.SemesterId = @SemesterId
              AND l.LessonDate >= @StartDate 
              AND l.LessonDate <= @EndDate
              AND ta.IsActive = 1 AND ta.IsDeleted = 0
              AND (
                  @Role IN ('Admin', 'Director')
                  OR
                  (@Role = 'Teacher' AND EXISTS (
                      SELECT 1 FROM [Core].[Teachers] t 
                      WHERE t.TeacherId = ta.TeacherId AND t.UserId = @UserId AND t.IsActive = 1 AND t.IsDeleted = 0
                  ))
                  OR
                  (@Role = 'Student' AND EXISTS (
                      SELECT 1 FROM [Core].[Students] s
                      WHERE s.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                      AND (ta.SubgroupId IS NULL OR EXISTS (
                          SELECT 1 FROM [Core].[StudentSubgroups] ss 
                          WHERE ss.StudentId = s.StudentId AND ss.SubgroupId = ta.SubgroupId
                      ))
                  ))
                  OR
                  (@Role = 'Parent' AND EXISTS (
                      SELECT 1 FROM [Core].[Parents] p
                      INNER JOIN [Core].[StudentParents] sp ON p.ParentId = sp.ParentId
                      INNER JOIN [Core].[Students] s ON sp.StudentId = s.StudentId
                      WHERE p.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                      AND (ta.SubgroupId IS NULL OR EXISTS (
                          SELECT 1 FROM [Core].[StudentSubgroups] ss 
                          WHERE ss.SubgroupId = ta.SubgroupId AND ss.StudentId = s.StudentId
                      ))
                  ))
              )
            ORDER BY l.LessonDate ASC, bs.StartTime ASC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var rawResults = await connection.QueryAsync<dynamic>(new CommandDefinition(
            sql,
            new
            {
                StartDate = startDate,
                EndDate = endDate,
                SemesterId = semesterId,
                UserId = userId,
                Role = role
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return rawResults.Select(rawResult => new LessonResponse(
                    rawResult.LessonId, rawResult.AssignmentId, rawResult.LessonDate, rawResult.LessonTopic, rawResult.Homework,
                    rawResult.LessonTypeId, rawResult.LessonTypeName, rawResult.PeriodId, rawResult.PeriodName,
                    rawResult.RoomId, rawResult.RoomName, rawResult.SemesterId,
                    Convert.ToBase64String((byte[])rawResult.RowVersion)
                ));
    }

    public async Task<IEnumerable<LessonResponse>> GetRoomOccupancyAsync(Guid roomId, DateTimeOffset lessonDate, Guid? periodId, Guid userId, string role, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                l.LessonId, l.AssignmentId, l.LessonDate, l.LessonTopic, l.Homework, l.LessonTypeId,
                lt.TypeName AS LessonTypeName,
                l.PeriodId, CAST(bs.LessonNumber AS NVARCHAR(10)) AS PeriodName,
                l.RoomId, r.RoomNumber AS RoomName,
                l.SemesterId, l.RowVersion
            FROM [Operations].[Lessons] l
            INNER JOIN [Operations].[TeachingAssignments] ta ON l.AssignmentId = ta.AssignmentId
            INNER JOIN [Reference].[LessonTypes] lt ON l.LessonTypeId = lt.LessonTypeId
            INNER JOIN [Reference].[BellSchedules] bs ON l.PeriodId = bs.ScheduleId
            INNER JOIN [Reference].[Classrooms] r ON l.RoomId = r.RoomId
            WHERE l.IsDeleted = 0 
              AND l.RoomId = @RoomId
              AND CAST(l.LessonDate AS DATE) = CAST(@LessonDate AS DATE)
              AND (@PeriodId IS NULL OR l.PeriodId = @PeriodId)
              AND ta.IsActive = 1 AND ta.IsDeleted = 0
              AND (
                  @Role IN ('Admin', 'Director')
                  OR
                  (@Role = 'Teacher' AND EXISTS (
                      SELECT 1 FROM [Core].[Teachers] t 
                      WHERE t.TeacherId = ta.TeacherId AND t.UserId = @UserId AND t.IsActive = 1 AND t.IsDeleted = 0
                  ))
                  OR
                  (@Role = 'Student' AND EXISTS (
                      SELECT 1 FROM [Core].[Students] s
                      WHERE s.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                      AND (ta.SubgroupId IS NULL OR EXISTS (
                          SELECT 1 FROM [Core].[StudentSubgroups] ss 
                          WHERE ss.StudentId = s.StudentId AND ss.SubgroupId = ta.SubgroupId
                      ))
                  ))
                  OR
                  (@Role = 'Parent' AND EXISTS (
                      SELECT 1 FROM [Core].[Parents] p
                      INNER JOIN [Core].[StudentParents] sp ON p.ParentId = sp.ParentId
                      INNER JOIN [Core].[Students] s ON sp.StudentId = s.StudentId
                      WHERE p.UserId = @UserId AND s.ClassId = ta.ClassId AND s.IsActive = 1 AND s.IsDeleted = 0
                      AND (ta.SubgroupId IS NULL OR EXISTS (
                          SELECT 1 FROM [Core].[StudentSubgroups] ss 
                          WHERE ss.SubgroupId = ta.SubgroupId AND ss.StudentId = s.StudentId
                      ))
                  ))
              )
            ORDER BY bs.StartTime ASC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var rawResults = await connection.QueryAsync<dynamic>(new CommandDefinition(
            sql,
            new
            {
                RoomId = roomId,
                LessonDate = lessonDate,
                PeriodId = periodId,
                UserId = userId,
                Role = role
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        return rawResults.Select(rawResult => new LessonResponse(
            rawResult.LessonId, rawResult.AssignmentId, rawResult.LessonDate, rawResult.LessonTopic, rawResult.Homework,
            rawResult.LessonTypeId, rawResult.LessonTypeName, rawResult.PeriodId, rawResult.PeriodName,
            rawResult.RoomId, rawResult.RoomName, rawResult.SemesterId,
            Convert.ToBase64String((byte[])rawResult.RowVersion)
        ));
    }
}