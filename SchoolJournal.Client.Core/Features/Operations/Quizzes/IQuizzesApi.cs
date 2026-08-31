using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Client.Core.Features.Operations.Quizzes;

public interface IQuizzesApi
{
    [Get("/api/quizzes")]
    public Task<IApiResponse<PagedResponse<QuizResponse>>> GetQuizzesPagedAsync(
        [Query] string? searchTerm,
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/teachers/{teacherId}/quizzes")]
    public Task<IApiResponse<PagedResponse<QuizResponse>>> GetQuizzesByTeacherAsync(
        Guid teacherId,
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/subjects/{subjectId}/quizzes")]
    public Task<IApiResponse<PagedResponse<QuizResponse>>> GetQuizzesBySubjectAsync(
        Guid subjectId,
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/quizzes/{id}")]
    public Task<IApiResponse<QuizDetailResponse>> GetQuizByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/quizzes")]
    public Task<IApiResponse<object>> CreateQuizAsync([Body] CreateQuizRequest request, CancellationToken ct = default);

    [Post("/api/quizzes/save-generated")]
    public Task<IApiResponse<object>> SaveGeneratedQuizAsync([Body] SaveGeneratedQuizRequest request, CancellationToken ct = default);

    [Put("/api/quizzes/{id}")]
    public Task<IApiResponse> UpdateQuizAsync(Guid id, [Body] UpdateQuizRequest request, CancellationToken ct = default);

    [Delete("/api/quizzes/{id}")]
    public Task<IApiResponse> DeleteQuizAsync(Guid id, [Body] DeleteQuizRequest request, CancellationToken ct = default);
}