using Dapper;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Core.Models;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Core.Repositories;

public sealed class StudentSubgroupRepository(SqlConnectionFactory connectionFactory) : IStudentSubgroupRepository
{
    public async Task<Guid> AddAsync(StudentSubgroup studentSubgroup, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentSubgroup);

        const string sql = """
            INSERT INTO [Core].[StudentSubgroups] (StudentId, SubgroupId, IsDeleted, CreatedAt)
            OUTPUT INSERTED.StudentSubgroupId
            VALUES (@StudentId, @SubgroupId, 0, GETUTCDATE());
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { studentSubgroup.StudentId, studentSubgroup.SubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsActiveAsync(Guid studentId, Guid subgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[StudentSubgroups]
                WHERE StudentId = @StudentId AND SubgroupId = @SubgroupId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { StudentId = studentId, SubgroupId = subgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentSubgroup?> GetByIdAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Core].[StudentSubgroups] WHERE StudentSubgroupId = @Id;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<StudentSubgroup>(new CommandDefinition(
            sql,
            new { Id = studentSubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentSubgroup?> DeleteAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE [Core].[StudentSubgroups]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE StudentSubgroupId = @Id AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentSubgroup>(new CommandDefinition(
            sql,
            new { Id = studentSubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentSubgroup?> UpdateSubgroupIdAsync(Guid studentSubgroupId, Guid newSubgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE [Core].[StudentSubgroups]
            SET SubgroupId = @NewSubgroupId,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE StudentSubgroupId = @Id AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentSubgroup>(new CommandDefinition(
            sql,
            new { Id = studentSubgroupId, NewSubgroupId = newSubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<StudentSubgroup?> RestoreAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE [Core].[StudentSubgroups]
            SET IsDeleted = 0,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE StudentSubgroupId = @Id AND IsDeleted = 1;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<StudentSubgroup>(new CommandDefinition(
            sql,
            new { Id = studentSubgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SubgroupStudentItem>> GetStudentsBySubgroupIdAsync(Guid subgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ss.StudentSubgroupId,
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.MiddleName AS Patronymic
            FROM [Core].[StudentSubgroups] ss
            INNER JOIN [Core].[Students] s ON ss.StudentId = s.StudentId
            WHERE ss.SubgroupId = @SubgroupId 
              AND ss.IsDeleted = 0
              AND s.IsDeleted = 0
            ORDER BY s.LastName, s.FirstName;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<SubgroupStudentItem>(new CommandDefinition(
            sql,
            new { SubgroupId = subgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<StudentSubgroupItem>> GetSubgroupsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ss.StudentSubgroupId,
                sg.SubgroupId,
                sg.SubgroupName,
                sg.ClassId,
                sg.SubjectId
            FROM [Core].[StudentSubgroups] ss
            INNER JOIN [Core].[Subgroups] sg ON ss.SubgroupId = sg.SubgroupId
            WHERE ss.StudentId = @StudentId 
              AND ss.IsDeleted = 0
              AND sg.IsDeleted = 0
              AND sg.IsActive = 1
            ORDER BY sg.SubgroupName;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<StudentSubgroupItem>(new CommandDefinition(
                    sql,
                    new { StudentId = studentId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<AvailableStudentItem>> GetAvailableStudentsForSubgroupIdAsync(Guid subgroupId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.MiddleName AS Patronymic
            FROM [Core].[Students] s
            INNER JOIN [Core].[Subgroups] sg ON sg.SubgroupId = @SubgroupId
            WHERE s.ClassId = sg.ClassId 
              AND s.IsDeleted = 0
              AND NOT EXISTS (
                  SELECT 1 FROM [Core].[StudentSubgroups] ss
                  WHERE ss.StudentId = s.StudentId 
                    AND ss.SubgroupId = @SubgroupId 
                    AND ss.IsDeleted = 0
              )
            ORDER BY s.LastName, s.FirstName;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<AvailableStudentItem>(new CommandDefinition(
            sql,
            new { SubgroupId = subgroupId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}