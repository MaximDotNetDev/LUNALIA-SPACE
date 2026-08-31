using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class TeachingAssignmentRepository(SqlConnectionFactory connectionFactory) : ITeachingAssignmentRepository
{
    public async Task<Guid> AddAsync(TeachingAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        const string sql = """
            INSERT INTO [Operations].[TeachingAssignments] (TeacherId, SubjectId, ClassId, SubgroupId)
            OUTPUT INSERTED.AssignmentId
            VALUES (@TeacherId, @SubjectId, @ClassId, @SubgroupId);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { assignment.TeacherId, assignment.SubjectId, assignment.ClassId, assignment.SubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(Guid teacherId, Guid subjectId, Guid classId, Guid? subgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeachingAssignments] 
                WHERE TeacherId = @TeacherId 
                  AND SubjectId = @SubjectId 
                  AND ClassId = @ClassId 
                  AND (SubgroupId = @SubgroupId OR (SubgroupId IS NULL AND @SubgroupId IS NULL))
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, SubjectId = subjectId, ClassId = classId, SubgroupId = subgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsExcludingIdAsync(Guid teacherId, Guid subjectId, Guid classId, Guid? subgroupId, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeachingAssignments] 
                WHERE TeacherId = @TeacherId 
                  AND SubjectId = @SubjectId 
                  AND ClassId = @ClassId 
                  AND (SubgroupId = @SubgroupId OR (SubgroupId IS NULL AND @SubgroupId IS NULL))
                  AND AssignmentId != @ExcludeId
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, SubjectId = subjectId, ClassId = classId, SubgroupId = subgroupId, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeachingAssignment?> UpdateAsync(TeachingAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        const string sql = """
            UPDATE [Operations].[TeachingAssignments]
            SET TeacherId = @TeacherId,
                SubjectId = @SubjectId,
                ClassId = @ClassId,
                SubgroupId = @SubgroupId,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AssignmentId = @AssignmentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TeachingAssignment>(new CommandDefinition(
            sql,
            new
            {
                assignment.TeacherId,
                assignment.SubjectId,
                assignment.ClassId,
                assignment.SubgroupId,
                assignment.AssignmentId,
                RowVersion = assignment.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeachingAssignment?> ToggleStatusAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[TeachingAssignments]
            SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AssignmentId = @AssignmentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TeachingAssignment>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeachingAssignment?> DeleteAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[TeachingAssignments]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AssignmentId = @AssignmentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TeachingAssignment>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeachingAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[TeachingAssignments] WHERE AssignmentId = @AssignmentId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<TeachingAssignment>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] 
            WHERE TeacherId = @TeacherId AND IsDeleted = 0;

            SELECT * FROM [Operations].[TeachingAssignments]
            WHERE TeacherId = @TeacherId AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<TeachingAssignment>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedByClassIdAsync(Guid classId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] 
            WHERE ClassId = @ClassId AND IsDeleted = 0;

            SELECT * FROM [Operations].[TeachingAssignments]
            WHERE ClassId = @ClassId AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { ClassId = classId, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<TeachingAssignment>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[TeachingAssignments] 
            WHERE SubjectId = @SubjectId AND IsDeleted = 0;

            SELECT * FROM [Operations].[TeachingAssignments]
            WHERE SubjectId = @SubjectId AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { SubjectId = subjectId, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<TeachingAssignment>().ConfigureAwait(false);

        return (items, totalCount);
    }
}