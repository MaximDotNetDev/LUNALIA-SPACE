using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Core.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class SchoolClassRepository(SqlConnectionFactory connectionFactory) : ISchoolClassRepository
{
    public async Task<Guid> AddAsync(SchoolClass schoolClass, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schoolClass);

        const string sql = """
            INSERT INTO [Core].[Classes] (ClassName, GradeLevel, AcademicYear, HomeroomTeacherId, IsActive)
            OUTPUT INSERTED.ClassId
            VALUES (@ClassName, @GradeLevel, @AcademicYear, @HomeroomTeacherId, @IsActive);
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { schoolClass.ClassName, schoolClass.GradeLevel, schoolClass.AcademicYear, schoolClass.HomeroomTeacherId, schoolClass.IsActive },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAndYearAsync(string className, string academicYear, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [Core].[Classes] WHERE ClassName = @ClassName AND AcademicYear = @AcademicYear AND IsDeleted = 0) THEN 1 ELSE 0 END;";
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { ClassName = className, AcademicYear = academicYear }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> TeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM [Core].[Teachers] WHERE TeacherId = @TeacherId AND IsDeleted = 0) THEN 1 ELSE 0 END;";
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { TeacherId = teacherId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClass?> GetByIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Classes] WHERE ClassId = @ClassId;";
        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<SchoolClass>(new CommandDefinition(sql, new { ClassId = classId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClass?> UpdateAsync(SchoolClass schoolClass, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schoolClass);

        const string sql = """
            UPDATE [Core].[Classes]
            SET ClassName = @ClassName,
                GradeLevel = @GradeLevel,
                AcademicYear = @AcademicYear,
                HomeroomTeacherId = @HomeroomTeacherId,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ClassId = @ClassId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SchoolClass>(new CommandDefinition(
            sql,
            new
            {
                schoolClass.ClassName,
                schoolClass.GradeLevel,
                schoolClass.AcademicYear,
                schoolClass.HomeroomTeacherId,
                schoolClass.ClassId,
                RowVersion = schoolClass.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameAndYearExcludingIdAsync(string className, string academicYear, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Classes] 
                WHERE ClassName = @ClassName 
                  AND AcademicYear = @AcademicYear 
                  AND ClassId != @ExcludeId 
                  AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { ClassName = className, AcademicYear = academicYear, ExcludeId = excludeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClass?> UpdateHomeroomTeacherAsync(Guid classId, Guid newTeacherId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Classes]
            SET HomeroomTeacherId = @HomeroomTeacherId,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ClassId = @ClassId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SchoolClass>(new CommandDefinition(
                    sql,
                    new
                    {
                        HomeroomTeacherId = newTeacherId,
                        ClassId = classId,
                        RowVersion = rowVersion
                    },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClass?> ChangeActiveStatusAsync(Guid classId, bool isActive, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Classes]
            SET IsActive = @IsActive,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ClassId = @ClassId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SchoolClass>(new CommandDefinition(
                    sql,
                    new
                    {
                        IsActive = isActive,
                        ClassId = classId,
                        RowVersion = rowVersion
                    },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClass?> DeleteAsync(Guid classId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Classes]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE ClassId = @ClassId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<SchoolClass>(new CommandDefinition(
                    sql,
                    new { ClassId = classId, RowVersion = rowVersion },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<SchoolClassDetailsModel?> GetDetailsByIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                c.ClassId, 
                c.ClassName, 
                c.GradeLevel, 
                c.AcademicYear, 
                c.HomeroomTeacherId, 
                c.IsActive, 
                c.RowVersion,
                t.LastName AS HomeroomTeacherLastName, 
                t.FirstName AS HomeroomTeacherFirstName, 
                t.MiddleName AS HomeroomTeacherMiddleName
            FROM [Core].[Classes] c
            INNER JOIN [Core].[Teachers] t ON c.HomeroomTeacherId = t.TeacherId
            WHERE c.ClassId = @ClassId 
              AND c.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<SchoolClassDetailsModel>(new CommandDefinition(
                    sql,
                    new { ClassId = classId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<SchoolClassItemModel> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, string? academicYear, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) 
            FROM [Core].[Classes] c 
            WHERE c.IsDeleted = 0 AND c.IsActive = 1 
              AND (@AcademicYear IS NULL OR c.AcademicYear = @AcademicYear);

            SELECT 
                c.ClassId, 
                c.ClassName, 
                c.GradeLevel, 
                c.AcademicYear, 
                t.LastName AS HomeroomTeacherLastName, 
                t.FirstName AS HomeroomTeacherFirstName, 
                t.MiddleName AS HomeroomTeacherMiddleName,
                TRIM(t.LastName + ' ' + t.FirstName + ' ' + ISNULL(t.MiddleName, '')) AS HomeroomTeacherFullName
            FROM [Core].[Classes] c
            INNER JOIN [Core].[Teachers] t ON c.HomeroomTeacherId = t.TeacherId
            WHERE c.IsDeleted = 0 AND c.IsActive = 1
              AND (@AcademicYear IS NULL OR c.AcademicYear = @AcademicYear)
            ORDER BY c.GradeLevel ASC, c.ClassName ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { AcademicYear = academicYear, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<SchoolClassItemModel>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<IEnumerable<SchoolClassItemModel>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        // SQL шукає класи за ДВОМА умовами:
        // 1. Вчитель є класним керівником (HomeroomTeacherId)
        // 2. АБО вчитель має призначення на викладання у цьому класі (TeachingAssignments)
        const string sql = """
            SELECT 
                c.ClassId, 
                c.ClassName, 
                c.GradeLevel, 
                c.AcademicYear, 
                t.LastName AS HomeroomTeacherLastName, 
                t.FirstName AS HomeroomTeacherFirstName, 
                t.MiddleName AS HomeroomTeacherMiddleName,
                TRIM(t.LastName + ' ' + t.FirstName + ' ' + ISNULL(t.MiddleName, '')) AS HomeroomTeacherFullName
            FROM [Core].[Classes] c
            INNER JOIN [Core].[Teachers] t ON c.HomeroomTeacherId = t.TeacherId
            WHERE c.IsDeleted = 0 
              AND c.IsActive = 1
              AND (
                  c.HomeroomTeacherId = @TeacherId 
                  OR EXISTS (
                      SELECT 1 
                      FROM [Operations].[TeachingAssignments] ta 
                      WHERE ta.ClassId = c.ClassId 
                        AND ta.TeacherId = @TeacherId 
                        AND ta.IsActive = 1 
                        AND ta.IsDeleted = 0
                  )
              )
            ORDER BY c.GradeLevel ASC, c.ClassName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<SchoolClassItemModel>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}