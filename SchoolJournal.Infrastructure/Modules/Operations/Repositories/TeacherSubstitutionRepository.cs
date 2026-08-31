using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Entities.Operations.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class TeacherSubstitutionRepository(SqlConnectionFactory connectionFactory) : ITeacherSubstitutionRepository
{
    public async Task<Guid> AddAsync(TeacherSubstitution substitution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(substitution);

        const string sql = """
            INSERT INTO [Operations].[TeacherSubstitutions] 
                (AssignmentId, SubstituteTeacherId, StartDate, EndDate)
            OUTPUT INSERTED.SubstitutionId
            VALUES 
                (@AssignmentId, @SubstituteTeacherId, @StartDate, @EndDate);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                substitution.AssignmentId,
                substitution.SubstituteTeacherId,
                substitution.StartDate,
                substitution.EndDate
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingForAssignmentAsync(Guid assignmentId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeacherSubstitutions]
                WHERE AssignmentId = @AssignmentId 
                  AND IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingForTeacherAsync(Guid substituteTeacherId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeacherSubstitutions]
                WHERE SubstituteTeacherId = @SubstituteTeacherId 
                  AND IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { SubstituteTeacherId = substituteTeacherId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeacherSubstitutionDetailed?> GetByIdAsync(Guid substitutionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                ts.SubstitutionId, ts.AssignmentId, ts.SubstituteTeacherId, ts.StartDate, ts.EndDate, ts.IsDeleted, ts.CreatedAt, ts.UpdatedAt, ts.RowVersion,
                TRIM(st.LastName + ' ' + st.FirstName + ' ' + ISNULL(st.MiddleName, '')) AS SubstituteTeacherFullName,
                TRIM(mt.LastName + ' ' + mt.FirstName + ' ' + ISNULL(mt.MiddleName, '')) AS MainTeacherFullName,
                s.SubjectName,
                c.ClassName,
                sg.SubgroupName
            FROM [Operations].[TeacherSubstitutions] ts
            INNER JOIN [Operations].[TeachingAssignments] ta ON ts.AssignmentId = ta.AssignmentId
            INNER JOIN [Core].[Teachers] st ON ts.SubstituteTeacherId = st.TeacherId
            INNER JOIN [Core].[Teachers] mt ON ta.TeacherId = mt.TeacherId
            INNER JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
            INNER JOIN [Core].[Classes] c ON ta.ClassId = c.ClassId
            LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
            WHERE ts.SubstitutionId = @SubstitutionId;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<TeacherSubstitutionDetailed>(new CommandDefinition(
            sql,
            new { SubstitutionId = substitutionId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeacherSubstitution?> UpdateAsync(TeacherSubstitution substitution, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(substitution);

        const string sql = """
            UPDATE [Operations].[TeacherSubstitutions]
            SET AssignmentId = @AssignmentId,
                SubstituteTeacherId = @SubstituteTeacherId,
                StartDate = @StartDate,
                EndDate = @EndDate,
                UpdatedAt = @UpdatedAt
            OUTPUT DELETED.*
            WHERE SubstitutionId = @SubstitutionId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TeacherSubstitution>(new CommandDefinition(
            sql,
            new
            {
                substitution.AssignmentId,
                substitution.SubstituteTeacherId,
                substitution.StartDate,
                substitution.EndDate,
                substitution.UpdatedAt,
                substitution.SubstitutionId,
                RowVersion = substitution.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingForAssignmentExcludingIdAsync(Guid assignmentId, DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeSubstitutionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeacherSubstitutions]
                WHERE AssignmentId = @AssignmentId 
                  AND SubstitutionId != @ExcludeSubstitutionId
                  AND IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId, StartDate = startDate, EndDate = endDate, ExcludeSubstitutionId = excludeSubstitutionId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasOverlappingForTeacherExcludingIdAsync(Guid substituteTeacherId, DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeSubstitutionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeacherSubstitutions]
                WHERE SubstituteTeacherId = @SubstituteTeacherId 
                  AND SubstitutionId != @ExcludeSubstitutionId
                  AND IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { SubstituteTeacherId = substituteTeacherId, StartDate = startDate, EndDate = endDate, ExcludeSubstitutionId = excludeSubstitutionId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<TeacherSubstitution?> DeleteAsync(Guid substitutionId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[TeacherSubstitutions]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE SubstitutionId = @SubstitutionId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TeacherSubstitution>(new CommandDefinition(
            sql,
            new { SubstitutionId = substitutionId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TeacherSubstitutionDetailed>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                ts.SubstitutionId, ts.AssignmentId, ts.SubstituteTeacherId, ts.StartDate, ts.EndDate, ts.IsDeleted, ts.CreatedAt, ts.UpdatedAt, ts.RowVersion,
                TRIM(st.LastName + ' ' + st.FirstName + ' ' + ISNULL(st.MiddleName, '')) AS SubstituteTeacherFullName,
                TRIM(mt.LastName + ' ' + mt.FirstName + ' ' + ISNULL(mt.MiddleName, '')) AS MainTeacherFullName,
                s.SubjectName,
                c.ClassName,
                sg.SubgroupName
            FROM [Operations].[TeacherSubstitutions] ts
            INNER JOIN [Operations].[TeachingAssignments] ta ON ts.AssignmentId = ta.AssignmentId
            INNER JOIN [Core].[Teachers] st ON ts.SubstituteTeacherId = st.TeacherId
            INNER JOIN [Core].[Teachers] mt ON ta.TeacherId = mt.TeacherId
            INNER JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
            INNER JOIN [Core].[Classes] c ON ta.ClassId = c.ClassId
            LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
            WHERE ts.AssignmentId = @AssignmentId AND ts.IsDeleted = 0
            ORDER BY ts.StartDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<TeacherSubstitutionDetailed>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TeacherSubstitutionDetailed>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                ts.SubstitutionId, ts.AssignmentId, ts.SubstituteTeacherId, ts.StartDate, ts.EndDate, ts.IsDeleted, ts.CreatedAt, ts.UpdatedAt, ts.RowVersion,
                TRIM(st.LastName + ' ' + st.FirstName + ' ' + ISNULL(st.MiddleName, '')) AS SubstituteTeacherFullName,
                TRIM(mt.LastName + ' ' + mt.FirstName + ' ' + ISNULL(mt.MiddleName, '')) AS MainTeacherFullName,
                s.SubjectName,
                c.ClassName,
                sg.SubgroupName
            FROM [Operations].[TeacherSubstitutions] ts
            INNER JOIN [Operations].[TeachingAssignments] ta ON ts.AssignmentId = ta.AssignmentId
            INNER JOIN [Core].[Teachers] st ON ts.SubstituteTeacherId = st.TeacherId
            INNER JOIN [Core].[Teachers] mt ON ta.TeacherId = mt.TeacherId
            INNER JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
            INNER JOIN [Core].[Classes] c ON ta.ClassId = c.ClassId
            LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
            WHERE ts.SubstituteTeacherId = @TeacherId AND ts.IsDeleted = 0
            ORDER BY ts.StartDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<TeacherSubstitutionDetailed>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TeacherSubstitutionDetailed>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                ts.SubstitutionId, ts.AssignmentId, ts.SubstituteTeacherId, ts.StartDate, ts.EndDate, ts.IsDeleted, ts.CreatedAt, ts.UpdatedAt, ts.RowVersion,
                TRIM(st.LastName + ' ' + st.FirstName + ' ' + ISNULL(st.MiddleName, '')) AS SubstituteTeacherFullName,
                TRIM(mt.LastName + ' ' + mt.FirstName + ' ' + ISNULL(mt.MiddleName, '')) AS MainTeacherFullName,
                s.SubjectName,
                c.ClassName,
                sg.SubgroupName
            FROM [Operations].[TeacherSubstitutions] ts
            INNER JOIN [Operations].[TeachingAssignments] ta ON ts.AssignmentId = ta.AssignmentId
            INNER JOIN [Core].[Teachers] st ON ts.SubstituteTeacherId = st.TeacherId
            INNER JOIN [Core].[Teachers] mt ON ta.TeacherId = mt.TeacherId
            INNER JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
            INNER JOIN [Core].[Classes] c ON ta.ClassId = c.ClassId
            LEFT JOIN [Core].[Subgroups] sg ON ta.SubgroupId = sg.SubgroupId
            WHERE ts.IsDeleted = 0 
              AND (GETUTCDATE() >= ts.StartDate AND GETUTCDATE() <= ts.EndDate)
            ORDER BY ts.StartDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<TeacherSubstitutionDetailed>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}