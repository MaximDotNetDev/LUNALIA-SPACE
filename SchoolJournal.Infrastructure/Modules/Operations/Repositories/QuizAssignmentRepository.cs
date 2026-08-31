using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class QuizAssignmentRepository(SqlConnectionFactory connectionFactory) : IQuizAssignmentRepository
{
    public async Task<Guid> AddAsync(QuizAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        const string sql = """
            INSERT INTO [Operations].[QuizAssignments] (QuizId, ClassId, AssignedDate, DueDate)
            OUTPUT INSERTED.AssignmentId
            VALUES (@QuizId, @ClassId, @AssignedDate, @DueDate);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                assignment.QuizId,
                assignment.ClassId,
                assignment.AssignedDate,
                assignment.DueDate
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Operations].[QuizAssignments] 
            WHERE AssignmentId = @AssignmentId;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<QuizAssignment>(new CommandDefinition(
            sql,
            new { AssignmentId = assignmentId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizAssignment?> UpdateAsync(QuizAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        const string sql = """
            UPDATE [Operations].[QuizAssignments]
            SET DueDate = @DueDate,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE AssignmentId = @AssignmentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<QuizAssignment>(new CommandDefinition(
            sql,
            new
            {
                assignment.DueDate,
                assignment.UpdatedAt,
                assignment.AssignmentId,
                RowVersion = assignment.RowVersion.ToArray()
            },
cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizAssignment?> DeleteAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[QuizAssignments]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE AssignmentId = @AssignmentId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<QuizAssignment>(new CommandDefinition(
                    sql,
                    new { AssignmentId = assignmentId, RowVersion = rowVersion },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> TeacherTeachesClassAsync(Guid teacherId, Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[TeachingAssignments]
                WHERE TeacherId = @TeacherId AND ClassId = @ClassId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId, ClassId = classId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.QuizAssignmentDetailsResult>> GetActiveByClassIdAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                qa.*, 
                q.Title AS QuizTitle,
                c.ClassName
            FROM [Operations].[QuizAssignments] qa
            INNER JOIN [Operations].[Quizzes] q ON qa.QuizId = q.QuizId
            INNER JOIN [Core].[Classes] c ON qa.ClassId = c.ClassId
            WHERE qa.ClassId = @ClassId AND qa.IsDeleted = 0 AND q.IsDeleted = 0
            ORDER BY qa.AssignedDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<SchoolJournal.Domain.Entities.Operations.Models.QuizAssignmentDetailsResult>(
            new CommandDefinition(sql, new { ClassId = classId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.QuizAssignmentDetailsResult>> GetActiveByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                qa.*, 
                q.Title AS QuizTitle,
                c.ClassName
            FROM [Operations].[QuizAssignments] qa
            INNER JOIN [Operations].[Quizzes] q ON qa.QuizId = q.QuizId
            INNER JOIN [Core].[Classes] c ON qa.ClassId = c.ClassId
            WHERE qa.QuizId = @QuizId AND qa.IsDeleted = 0 AND q.IsDeleted = 0
            ORDER BY qa.AssignedDate DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<SchoolJournal.Domain.Entities.Operations.Models.QuizAssignmentDetailsResult>(
            new CommandDefinition(sql, new { QuizId = quizId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Guid> GetSubjectIdByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT q.SubjectId 
            FROM [Operations].[QuizAssignments] qa
            INNER JOIN [Operations].[Quizzes] q ON qa.QuizId = q.QuizId
            WHERE qa.AssignmentId = @AssignmentId AND qa.IsDeleted = 0;
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql, new { AssignmentId = assignmentId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}