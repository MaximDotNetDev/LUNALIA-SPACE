using Dapper;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Core.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class StudentRepository(SqlConnectionFactory connectionFactory) : IStudentRepository
{
    public async Task<Guid> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);

        const string sql = """
            INSERT INTO [Core].[Students] (
                LastName, FirstName, MiddleName, DateOfBirth, ClassId, Gender, 
                DocumentType, DocumentSeries, DocumentNumber, EnrollmentDate, 
                EnrollmentReason, Address, MedicalNotes, UserId, IsActive, IsDeleted
            )
            OUTPUT INSERTED.StudentId
            VALUES (
                @LastName, @FirstName, @MiddleName, @DateOfBirth, @ClassId, @Gender, 
                @DocumentType, @DocumentSeries, @DocumentNumber, @EnrollmentDate, 
                @EnrollmentReason, @Address, @MedicalNotes, @UserId, 1, 0
            );
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                student.LastName,
                student.FirstName,
                student.MiddleName,
                student.DateOfBirth,
                student.ClassId,
                Gender = student.Gender.ToString(),
                student.DocumentType,
                student.DocumentSeries,
                student.DocumentNumber,
                student.EnrollmentDate,
                student.EnrollmentReason,
                student.Address,
                student.MedicalNotes,
                student.UserId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Students] WHERE StudentId = @StudentId;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Student>(new CommandDefinition(
            sql, new { StudentId = studentId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByDocumentAsync(string type, string number, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] 
                WHERE DocumentType = @Type AND DocumentNumber = @Number AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { Type = type, Number = number }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByDocumentExcludingIdAsync(string type, string number, Guid excludeId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] 
                WHERE DocumentType = @Type AND DocumentNumber = @Number AND StudentId != @ExcludeId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { Type = type, Number = number, ExcludeId = excludeId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);

        const string sql = """
            UPDATE [Core].[Students]
            SET LastName = @LastName,
                FirstName = @FirstName,
                MiddleName = @MiddleName,
                DateOfBirth = @DateOfBirth,
                ClassId = @ClassId,
                Gender = @Gender,
                DocumentType = @DocumentType,
                DocumentSeries = @DocumentSeries,
                DocumentNumber = @DocumentNumber,
                EnrollmentDate = @EnrollmentDate,
                EnrollmentReason = @EnrollmentReason,
                Address = @Address,
                MedicalNotes = @MedicalNotes,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentId = @StudentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Student>(new CommandDefinition(
            sql,
            new
            {
                student.LastName,
                student.FirstName,
                student.MiddleName,
                student.DateOfBirth,
                student.ClassId,
                Gender = student.Gender.ToString(),
                student.DocumentType,
                student.DocumentSeries,
                student.DocumentNumber,
                student.EnrollmentDate,
                student.EnrollmentReason,
                student.Address,
                student.MedicalNotes,
                student.StudentId,
                RowVersion = student.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> DeleteAsync(Guid studentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Students]
            SET IsDeleted = 1,
                IsActive = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentId = @StudentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Student>(new CommandDefinition(
            sql,
            new { StudentId = studentId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> TransferToClassAsync(Guid studentId, Guid newClassId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Students]
            SET ClassId = @NewClassId,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentId = @StudentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Student>(new CommandDefinition(
            sql,
            new { StudentId = studentId, NewClassId = newClassId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> IsUserAlreadyLinkedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Students] 
                WHERE UserId = @UserId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { UserId = userId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> LinkUserAsync(Guid studentId, Guid userId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Students]
            SET UserId = @UserId,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentId = @StudentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Student>(new CommandDefinition(
            sql,
            new { StudentId = studentId, UserId = userId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> UpdateMedicalNotesAsync(Guid studentId, string? medicalNotes, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Core].[Students]
            SET MedicalNotes = @MedicalNotes,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentId = @StudentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Student>(new CommandDefinition(
            sql,
            new { StudentId = studentId, MedicalNotes = medicalNotes, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Student>> GetActiveByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Core].[Students]
            WHERE ClassId = @ClassId 
              AND IsDeleted = 0 
            ORDER BY LastName, FirstName, MiddleName;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Student>(new CommandDefinition(
            sql,
            new { ClassId = classId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<StudentSearchResult> Items, int TotalCount)> SearchAsync(
            string? searchTerm,
            Guid? classId,
            bool? isActive,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
    {
        // Додаємо аліас "s.", оскільки тепер у нас дві таблиці
        var whereClauses = new List<string> { "s.IsDeleted = 0" };
        var parameters = new DynamicParameters();
        parameters.Add("Skip", skip);
        parameters.Add("Take", take);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(s.LastName LIKE @Search OR s.FirstName LIKE @Search OR s.MiddleName LIKE @Search)");
            parameters.Add("Search", $"%{searchTerm.Trim()}%");
        }

        if (classId.HasValue)
        {
            whereClauses.Add("s.ClassId = @ClassId");
            parameters.Add("ClassId", classId.Value);
        }

        if (isActive.HasValue)
        {
            whereClauses.Add("s.IsActive = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        var whereSql = string.Join(" AND ", whereClauses);

        var sql = $"""
            SELECT COUNT(*) 
            FROM [Core].[Students] s 
            WHERE {whereSql};

            SELECT 
                s.StudentId, s.LastName, s.FirstName, s.MiddleName, 
                s.ClassId, s.IsActive, s.CreatedAt, 
                s.UserId, u.Login
            FROM [Core].[Students] s
            LEFT JOIN [Identity].[Users] u ON s.UserId = u.UserId
            WHERE {whereSql}
            ORDER BY s.LastName, s.FirstName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql, parameters, cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        // Читаємо дані у нашу нову модель
        var items = await multi.ReadAsync<StudentSearchResult>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<IEnumerable<StudentHistory>> GetHistoryAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                StudentId, LastName, FirstName, MiddleName, ClassId, 
                IsActive, IsDeleted, SysStartTime AS ValidFrom, SysEndTime AS ValidTo
            FROM [Core].[Students]
            FOR SYSTEM_TIME ALL
            WHERE StudentId = @StudentId
            ORDER BY SysStartTime DESC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<StudentHistory>(new CommandDefinition(
            sql,
            new { StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[Students] WHERE UserId = @UserId AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Student>(new CommandDefinition(
            sql, new { UserId = userId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}