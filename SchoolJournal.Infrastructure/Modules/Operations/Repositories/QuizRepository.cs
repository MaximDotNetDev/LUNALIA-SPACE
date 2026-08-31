using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class QuizRepository(SqlConnectionFactory connectionFactory) : IQuizRepository
{
    public async Task<Guid> AddGeneratedQuizAsync(Quiz quiz, IEnumerable<QuizQuestion> questions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        ArgumentNullException.ThrowIfNull(questions);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertQuizSql = """
                INSERT INTO [Operations].[Quizzes] (TeacherId, SubjectId, Title)
                OUTPUT INSERTED.QuizId
                VALUES (@TeacherId, @SubjectId, @Title);
                """;

            var quizId = await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
                insertQuizSql,
                new { quiz.TeacherId, quiz.SubjectId, quiz.Title },
                transaction: transaction,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            const string insertQuestionSql = """
                INSERT INTO [Operations].[QuizQuestions] (QuizId, OrderIndex, QuestionText, QuestionType, ContentJson, Points)
                VALUES (@QuizId, @OrderIndex, @QuestionText, @QuestionType, @ContentJson, @Points);
                """;

            var questionsParam = questions.Select(q => new
            {
                QuizId = quizId,
                q.OrderIndex,
                q.QuestionText,
                q.QuestionType,
                q.ContentJson,
                q.Points
            }).ToArray();

            await connection.ExecuteAsync(new CommandDefinition(
                insertQuestionSql,
                questionsParam,
                transaction: transaction,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            transaction.Commit();
            return quizId;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Guid> AddAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        const string sql = """
            INSERT INTO [Operations].[Quizzes] (TeacherId, SubjectId, Title)
            OUTPUT INSERTED.QuizId
            VALUES (@TeacherId, @SubjectId, @Title);
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql,
            new { quiz.TeacherId, quiz.SubjectId, quiz.Title },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM [Operations].[Quizzes] WHERE QuizId = @QuizId;";

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Quiz>(new CommandDefinition(
            sql,
            new { QuizId = quizId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(Quiz? Quiz, IEnumerable<QuizQuestion> Questions)> GetWithQuestionsByIdAsync(Guid quizId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Operations].[Quizzes] 
            WHERE QuizId = @QuizId AND IsDeleted = 0;

            SELECT * FROM [Operations].[QuizQuestions] 
            WHERE QuizId = @QuizId AND IsDeleted = 0
            ORDER BY OrderIndex ASC;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { QuizId = quizId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var quiz = await multi.ReadSingleOrDefaultAsync<Quiz>().ConfigureAwait(false);
        if (quiz is null)
        {
            return (null, []);
        }

        var questions = await multi.ReadAsync<QuizQuestion>().ConfigureAwait(false);

        return (quiz, questions);
    }

    public async Task<bool> TeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Teachers] WHERE TeacherId = @TeacherId
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { TeacherId = teacherId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Quiz?> UpdateWithQuestionsAsync(Quiz quiz, IEnumerable<QuizQuestion> questions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        ArgumentNullException.ThrowIfNull(questions);

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string updateQuizSql = """
                UPDATE [Operations].[Quizzes]
                SET SubjectId = @SubjectId,
                    Title = @Title,
                    UpdatedAt = GETUTCDATE()
                OUTPUT INSERTED.*
                WHERE QuizId = @QuizId 
                  AND RowVersion = @RowVersion 
                  AND IsDeleted = 0;
                """;

            var updatedQuiz = await connection.QuerySingleOrDefaultAsync<Quiz>(new CommandDefinition(
                updateQuizSql,
                new { quiz.SubjectId, quiz.Title, quiz.QuizId, RowVersion = quiz.RowVersion.ToArray() },
                transaction: transaction,
                cancellationToken: cancellationToken)).ConfigureAwait(false);

            if (updatedQuiz is null)
            {
                return null;
            }

            var incomingQuestions = questions.ToList();
            var incomingIds = incomingQuestions.Where(q => q.QuestionId != Guid.Empty).Select(q => q.QuestionId).ToList();

            if (incomingIds.Count > 0)
            {
                const string deleteSql = """
                    UPDATE [Operations].[QuizQuestions]
                    SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
                    WHERE QuizId = @QuizId AND QuestionId NOT IN @IncomingIds AND IsDeleted = 0;
                    """;
                await connection.ExecuteAsync(new CommandDefinition(
                                    deleteSql,
                                    new { quiz.QuizId, IncomingIds = incomingIds },
                                    transaction: transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            }
            else
            {
                const string deleteAllSql = """
                    UPDATE [Operations].[QuizQuestions]
                    SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
                    WHERE QuizId = @QuizId AND IsDeleted = 0;
                    """;
                await connection.ExecuteAsync(new CommandDefinition(
                                    deleteAllSql,
                                    new { quiz.QuizId },
                                    transaction: transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            }

            var toUpdate = incomingQuestions.Where(q => q.QuestionId != Guid.Empty).ToList();
            if (toUpdate.Count > 0)
            {
                const string updateQuestionSql = """
                    UPDATE [Operations].[QuizQuestions]
                    SET OrderIndex = @OrderIndex, 
                        QuestionText = @QuestionText, 
                        QuestionType = @QuestionType, 
                        ContentJson = @ContentJson, 
                        Points = @Points, 
                        UpdatedAt = GETUTCDATE()
                    WHERE QuestionId = @QuestionId AND QuizId = @QuizId AND IsDeleted = 0;
                    """;
                await connection.ExecuteAsync(new CommandDefinition(
                    updateQuestionSql,
                    toUpdate,
                    transaction: transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            }

            var toInsert = incomingQuestions.Where(q => q.QuestionId == Guid.Empty).ToList();
            if (toInsert.Count > 0)
            {
                const string insertQuestionSql = """
                    INSERT INTO [Operations].[QuizQuestions] (QuizId, OrderIndex, QuestionText, QuestionType, ContentJson, Points)
                    VALUES (@QuizId, @OrderIndex, @QuestionText, @QuestionType, @ContentJson, @Points);
                    """;
                await connection.ExecuteAsync(new CommandDefinition(
                    insertQuestionSql,
                    toInsert,
                    transaction: transaction, cancellationToken: cancellationToken)).ConfigureAwait(false);
            }

            transaction.Commit();
            return updatedQuiz;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Quiz?> UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quiz);
        const string sql = """
            UPDATE [Operations].[Quizzes]
            SET SubjectId = @SubjectId,
                Title = @Title,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE QuizId = @QuizId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Quiz>(new CommandDefinition(
            sql,
            new
            {
                quiz.SubjectId,
                quiz.Title,
                quiz.QuizId,
                RowVersion = quiz.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Quiz?> DeleteAsync(Guid quizId, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rowVersion);

        const string sql = """
            UPDATE [Operations].[Quizzes]
            SET IsDeleted = 1,
                UpdatedAt = GETUTCDATE()
            OUTPUT INSERTED.*
            WHERE QuizId = @QuizId 
              AND RowVersion = @RowVersion 
              AND IsDeleted = 0;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Quiz>(new CommandDefinition(
            sql,
            new { QuizId = quizId, RowVersion = rowVersion.ToArray() },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[Quizzes] 
            WHERE TeacherId = @TeacherId AND IsDeleted = 0;

            SELECT * FROM [Operations].[Quizzes]
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
        var items = await multi.ReadAsync<Quiz>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*) FROM [Operations].[Quizzes] 
            WHERE SubjectId = @SubjectId AND IsDeleted = 0;

            SELECT * FROM [Operations].[Quizzes]
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
        var items = await multi.ReadAsync<Quiz>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default)
    {
        var likeTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%";

        const string sql = """
            SELECT COUNT(*) FROM [Operations].[Quizzes] 
            WHERE IsDeleted = 0 
              AND (@SearchTerm IS NULL OR Title LIKE @SearchTerm);

            SELECT * FROM [Operations].[Quizzes]
            WHERE IsDeleted = 0 
              AND (@SearchTerm IS NULL OR Title LIKE @SearchTerm)
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            """;

        using var connection = connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            sql,
            new { SearchTerm = likeTerm, Skip = skip, Take = take },
            cancellationToken: cancellationToken)).ConfigureAwait(false);

        var totalCount = await multi.ReadFirstAsync<int>().ConfigureAwait(false);
        var items = await multi.ReadAsync<Quiz>().ConfigureAwait(false);

        return (items, totalCount);
    }

    public async Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [Core].[Subjects] WHERE SubjectId = @SubjectId
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { SubjectId = subjectId },
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}