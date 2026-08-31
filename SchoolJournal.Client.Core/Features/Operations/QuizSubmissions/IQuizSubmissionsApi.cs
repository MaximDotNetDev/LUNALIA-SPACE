using Refit;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

namespace SchoolJournal.Client.Core.Features.Operations.QuizSubmissions;

public interface IQuizSubmissionsApi
{
    [Post("/api/quiz-submissions")]
    public Task<IApiResponse<SubmitQuizResponse>> SubmitQuizAsync([Body] SubmitQuizRequest request, CancellationToken ct = default);

    [Get("/api/quiz-submissions/assignment/{assignmentId}")]
    public Task<IApiResponse<List<QuizSubmissionResultDto>>> GetAssignmentSubmissionsAsync(Guid assignmentId, CancellationToken ct = default);
}