using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class GradeRepository(SqlConnectionFactory connectionFactory) : IGradeRepository
{
    public async Task<Guid> AddAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grade);

        const string sql = """
            INSERT INTO [Operations].[Grades] (
                LessonId, StudentId, GradeValue, Comment, 
                CreatedByUserId, UpdatedByUserId, GradeTypeId
            )
            OUTPUT INSERTED.GradeId
            VALUES (
                @LessonId, @StudentId, @GradeValue, @Comment, 
                @CreatedByUserId, @UpdatedByUserId, @GradeTypeId
            );
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                grade.LessonId,
                grade.StudentId,
                grade.GradeValue,
                grade.Comment,
                grade.CreatedByUserId,
                grade.UpdatedByUserId,
                grade.GradeTypeId
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Grade?> GetByIdAsync(Guid gradeId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Grades] WHERE GradeId = @GradeId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Grade>(new CommandDefinition(
            sql,
            new { GradeId = gradeId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Grade?> UpdateAsync(Grade grade, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grade);

        const string sql = """
            UPDATE [Operations].[Grades]
            SET GradeValue = @GradeValue,
                Comment = @Comment,
                GradeTypeId = @GradeTypeId,
                UpdatedByUserId = @UpdatedByUserId,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE GradeId = @GradeId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Grade>(new CommandDefinition(
            sql,
            new
            {
                grade.GradeValue,
                grade.Comment,
                grade.GradeTypeId,
                grade.UpdatedByUserId,
                grade.UpdatedAt,
                grade.GradeId,
                RowVersion = grade.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Grade?> DeleteAsync(Guid gradeId, Guid updatedByUserId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Grades]
            SET IsDeleted = 1,
                UpdatedByUserId = @UpdatedByUserId,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE GradeId = @GradeId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Grade>(new CommandDefinition(
            sql,
            new { GradeId = gradeId, UpdatedByUserId = updatedByUserId, UpdatedAt = DateTimeOffset.UtcNow, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Grade>> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Grades] WHERE LessonId = @LessonId AND IsDeleted = 0;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Grade>(new CommandDefinition(
            sql,
            new { LessonId = lessonId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Grade>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Grades] WHERE StudentId = @StudentId AND IsDeleted = 0 ORDER BY CreatedAt DESC;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Grade>(new CommandDefinition(
            sql,
            new { StudentId = studentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}