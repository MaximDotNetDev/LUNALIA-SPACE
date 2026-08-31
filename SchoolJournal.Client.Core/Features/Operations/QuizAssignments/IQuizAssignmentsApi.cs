using Refit;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

namespace SchoolJournal.Client.Core.Features.Operations.QuizAssignments;

public interface IQuizAssignmentsApi
{
    [Get("/api/quiz-assignments/{id}")]
    public Task<IApiResponse<QuizAssignmentResponse>> GetQuizAssignmentByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/quiz-assignments/class/{classId}")]
    public Task<IApiResponse<IReadOnlyCollection<QuizAssignmentResponse>>> GetActiveQuizAssignmentsByClassIdAsync(Guid classId, CancellationToken ct = default);

    [Get("/api/quiz-assignments/quiz/{quizId}")]
    public Task<IApiResponse<IReadOnlyCollection<QuizAssignmentResponse>>> GetActiveQuizAssignmentsByQuizIdAsync(Guid quizId, CancellationToken ct = default);

    [Post("/api/quiz-assignments")]
    public Task<IApiResponse<object>> CreateQuizAssignmentAsync([Body] CreateQuizAssignmentRequest request, CancellationToken ct = default);

    [Put("/api/quiz-assignments/{id}")]
    public Task<IApiResponse> UpdateQuizAssignmentDueDateAsync(Guid id, [Body] UpdateQuizAssignmentDueDateRequest request, CancellationToken ct = default);

    [Delete("/api/quiz-assignments/{id}")]
    public Task<IApiResponse> DeleteQuizAssignmentAsync(Guid id, [Body] DeleteQuizAssignmentRequest request, CancellationToken ct = default);
}