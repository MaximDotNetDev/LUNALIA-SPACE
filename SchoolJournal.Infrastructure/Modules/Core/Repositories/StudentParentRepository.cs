using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;
using System.Reflection;
using SchoolJournal.Domain.Entities.Core.Models; 

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class StudentParentRepository(SqlConnectionFactory connectionFactory) : IStudentParentRepository
{
    public async Task<Guid> AddAsync(StudentParent studentParent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentParent);

        const string sql = """
            INSERT INTO [Core].[StudentParents] (StudentId, ParentId, Role)
            OUTPUT INSERTED.StudentParentId
            VALUES (@StudentId, @ParentId, @Role);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { studentParent.StudentId, studentParent.ParentId, studentParent.Role },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(Guid studentId, Guid parentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[StudentParents] 
                WHERE StudentId = @StudentId AND ParentId = @ParentId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { StudentId = studentId, ParentId = parentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentParent?> GetByIdAsync(Guid studentParentId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[StudentParents] WHERE StudentParentId = @StudentParentId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<StudentParent>(new CommandDefinition(
                    sql,
                    new { StudentParentId = studentParentId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentParent?> UpdateRoleAsync(Guid studentParentId, string? role, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Core].[StudentParents]
            SET Role = @Role,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE StudentParentId = @StudentParentId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentParent>(new CommandDefinition(
                    sql,
                    new { StudentParentId = studentParentId, Role = role },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentParent?> DeleteAsync(Guid studentParentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Core].[StudentParents]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentParentId = @StudentParentId 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentParent>(new CommandDefinition(
                    sql,
                    new { StudentParentId = studentParentId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentParent?> RestoreAsync(Guid studentParentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Core].[StudentParents]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE StudentParentId = @StudentParentId 
              AND IsDeleted = 1;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentParent>(new CommandDefinition(
                    sql,
                    new { StudentParentId = studentParentId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<StudentParentDetail>> GetParentsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                sp.StudentParentId, sp.Role,
                p.ParentId, p.LastName, p.FirstName, p.MiddleName, p.Phone, p.UserId, 
                p.IsActive, p.IsDeleted, p.CreatedAt, p.UpdatedAt, p.RowVersion
            FROM [Core].[StudentParents] sp
            INNER JOIN [Core].[Parents] p ON sp.ParentId = p.ParentId
            WHERE sp.StudentId = @StudentId 
              AND sp.IsDeleted = 0 
              AND p.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<StudentParentDetail, Parent, StudentParentDetail>(
            new CommandDefinition(sql, new { StudentId = studentId }, cancellationToken: cancellationToken),
            (detail, parent) =>
            {
                return new StudentParentDetail
                {
                    StudentParentId = detail.StudentParentId,
                    Role = detail.Role,
                    Parent = parent
                };
            },
            splitOn: "ParentId"
        ).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ParentStudentDetail>> GetStudentsByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                sp.StudentParentId, sp.Role,
                s.StudentId, s.LastName, s.FirstName, s.MiddleName, s.DateOfBirth,
                s.ClassId, s.Gender, s.DocumentType, s.DocumentSeries, s.DocumentNumber,
                s.EnrollmentDate, s.EnrollmentReason, s.Address, s.MedicalNotes,
                s.UserId, s.IsActive, s.IsDeleted, s.CreatedAt, s.UpdatedAt, s.RowVersion
            FROM [Core].[StudentParents] sp
            INNER JOIN [Core].[Students] s ON sp.StudentId = s.StudentId
            WHERE sp.ParentId = @ParentId 
              AND sp.IsDeleted = 0 
              AND s.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<ParentStudentDetail, Student, ParentStudentDetail>(
            new CommandDefinition(sql, new { ParentId = parentId }, cancellationToken: cancellationToken),
            (detail, student) =>
            {
                return new ParentStudentDetail
                {
                    StudentParentId = detail.StudentParentId,
                    Role = detail.Role,
                    Student = student
                };
            },
            splitOn: "StudentId"
        ).ConfigureAwait(false);
    }
}