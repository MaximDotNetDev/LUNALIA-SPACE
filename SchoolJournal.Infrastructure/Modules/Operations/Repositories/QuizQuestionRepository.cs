using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class QuizQuestionRepository(SqlConnectionFactory connectionFactory) : IQuizQuestionRepository
{
    public async Task<Guid> AddAsync(QuizQuestion question, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);

        const string sql = """
            INSERT INTO [Operations].[QuizQuestions] (QuizId, OrderIndex, QuestionText, QuestionType, ContentJson, Points, IsDeleted, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.QuestionId
            VALUES (@QuizId, @OrderIndex, @QuestionText, @QuestionType, @ContentJson, @Points, @IsDeleted, @CreatedAt, @UpdatedAt);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new
            {
                question.QuizId,
                question.OrderIndex,
                question.QuestionText,
                question.QuestionType,
                question.ContentJson,
                question.Points,
                question.IsDeleted,
                question.CreatedAt,
                question.UpdatedAt
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<int> GetNextOrderIndexAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COALESCE(MAX(OrderIndex) + 1, 0)
            FROM [Operations].[QuizQuestions]
            WHERE QuizId = @QuizId AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { QuizId = quizId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizQuestion?> GetByIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[QuizQuestions] WHERE QuestionId = @QuestionId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<QuizQuestion>(new CommandDefinition(
            sql,
            new { QuestionId = questionId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizQuestion?> UpdateAsync(QuizQuestion question, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);

        const string sql = """
            UPDATE [Operations].[QuizQuestions]
            SET QuestionText = @QuestionText,
                QuestionType = @QuestionType,
                ContentJson = @ContentJson,
                Points = @Points,
                UpdatedAt = @UpdatedAt
            OUTPUT INSERTED.*
            WHERE QuestionId = @QuestionId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<QuizQuestion>(new CommandDefinition(
            sql,
            new
            {
                question.QuestionText,
                question.QuestionType,
                question.ContentJson,
                question.Points,
                question.UpdatedAt,
                question.QuestionId,
                RowVersion = question.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<QuizQuestion?> DeleteAsync(Guid questionId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[QuizQuestions]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT DELETED.*
            WHERE QuestionId = @QuestionId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<QuizQuestion>(new CommandDefinition(
            sql,
            new { QuestionId = questionId, RowVersion = rowVersion },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> ReorderAsync(Guid quizId, string ordersJson, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ordersJson);

        const string sql = """
            UPDATE q
            SET q.OrderIndex = j.OrderIndex,
                q.UpdatedAt = GETUTCDATE()
            FROM [Operations].[QuizQuestions] q
            INNER JOIN OPENJSON(@OrdersJson)
            WITH (
                QuestionId UNIQUEIDENTIFIER '$.QuestionId',
                OrderIndex INT '$.OrderIndex'
            ) j ON q.QuestionId = j.QuestionId
            WHERE q.QuizId = @QuizId AND q.IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
                    sql,
                    new { QuizId = quizId, OrdersJson = ordersJson },
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

        return affectedRows > 0;
    }

    public async Task<(IEnumerable<QuizQuestion> Items, int TotalCount)> GetPagedByQuizIdAsync(Guid quizId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[QuizQuestions] WHERE QuizId = @QuizId AND IsDeleted = 0;

            SELECT * FROM [Operations].[QuizQuestions]
            WHERE QuizId = @QuizId AND IsDeleted = 0
            ORDER BY OrderIndex ASC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { QuizId = quizId, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<QuizQuestion>().ConfigureAwait(false);

        return (items, totalCount);
    }
}