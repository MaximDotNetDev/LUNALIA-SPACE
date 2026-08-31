using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class QuizSubmissionRepository(SqlConnectionFactory connectionFactory) : IQuizSubmissionRepository
{
    public async Task<Guid> AddAsync(QuizSubmission submission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);

        const string sql = """
            INSERT INTO [Operations].[QuizSubmissions] 
                (AssignmentId, StudentId, Score, MaxScore, IsDeleted, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.SubmissionId
            VALUES 
                (@AssignmentId, @StudentId, @Score, @MaxScore, @IsDeleted, @CreatedAt, @UpdatedAt);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                submission.AssignmentId,
                submission.StudentId,
                submission.Score,
                submission.MaxScore,
                submission.IsDeleted,
                submission.CreatedAt,
                submission.UpdatedAt
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> HasStudentSubmittedAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Operations].[QuizSubmissions] 
                WHERE AssignmentId = @AssignmentId AND StudentId = @StudentId AND IsDeleted = 0
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                    sql,
                    new { AssignmentId = assignmentId, StudentId = studentId },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.QuizSubmissionResult>> GetAssignmentSubmissionsAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                qs.SubmissionId,
                (s.LastName + ' ' + s.FirstName) AS StudentFullName,
                qs.Score,
                qs.MaxScore,
                qs.CreatedAt AS SubmittedAt
            FROM [Operations].[QuizSubmissions] qs
            INNER JOIN [Core].[Students] s ON qs.StudentId = s.StudentId
            WHERE qs.AssignmentId = @AssignmentId AND qs.IsDeleted = 0
            ORDER BY qs.Score DESC, qs.CreatedAt DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        return await connection.QueryAsync<SchoolJournal.Domain.Entities.Operations.Models.QuizSubmissionResult>(
            new CommandDefinition(sql, new { AssignmentId = assignmentId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}