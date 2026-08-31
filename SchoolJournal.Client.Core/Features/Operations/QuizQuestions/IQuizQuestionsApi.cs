using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

namespace SchoolJournal.Client.Core.Features.Operations.QuizQuestions;

public interface IQuizQuestionsApi
{
    [Get("/api/quizzes/{quizId}/questions")]
    public Task<IApiResponse<PagedResponse<QuizQuestionResponse>>> GetQuestionsAsync(
        Guid quizId,
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/quiz-questions/{id}")]
    public Task<IApiResponse<QuizQuestionResponse>> GetQuestionByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/quizzes/{quizId}/questions")]
    public Task<IApiResponse<object>> CreateQuestionAsync(Guid quizId, [Body] CreateQuizQuestionRequest request, CancellationToken ct = default);

    [Put("/api/quiz-questions/{id}")]
    public Task<IApiResponse> UpdateQuestionAsync(Guid id, [Body] UpdateQuizQuestionRequest request, CancellationToken ct = default);

    [Delete("/api/quiz-questions/{id}")]
    public Task<IApiResponse> DeleteQuestionAsync(Guid id, [Body] DeleteQuizQuestionRequest request, CancellationToken ct = default);

    [Post("/api/quizzes/{quizId}/questions/reorder")]
    public Task<IApiResponse> ReorderQuestionsAsync(Guid quizId, [Body] ReorderQuizQuestionsRequest request, CancellationToken ct = default);
}