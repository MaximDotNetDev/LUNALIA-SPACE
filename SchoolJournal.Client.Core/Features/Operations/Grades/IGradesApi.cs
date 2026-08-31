using Refit;
using SchoolJournal.Contracts.DTOs.Operations.Grades;

namespace SchoolJournal.Client.Core.Features.Operations.Grades;

public interface IGradesApi
{
    [Get("/api/grades/{id}")]
    public Task<IApiResponse<GradeResponse>> GetGradeByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/lessons/{lessonId}/grades")]
    public Task<IApiResponse<IReadOnlyCollection<GradeResponse>>> GetGradesByLessonAsync(Guid lessonId, CancellationToken ct = default);

    [Get("/api/students/{studentId}/grades")]
    public Task<IApiResponse<IReadOnlyCollection<GradeResponse>>> GetGradesByStudentAsync(Guid studentId, CancellationToken ct = default);

    [Post("/api/grades")]
    public Task<IApiResponse<object>> CreateGradeAsync([Body] CreateGradeRequest request, CancellationToken ct = default);

    [Put("/api/grades/{id}")]
    public Task<IApiResponse> UpdateGradeAsync(Guid id, [Body] UpdateGradeRequest request, CancellationToken ct = default);

    [Delete("/api/grades/{id}")]
    public Task<IApiResponse> DeleteGradeAsync(Guid id, [Body] DeleteGradeRequest request, CancellationToken ct = default);

    [Post("/api/grades/{id}/boost")]
    public Task<IApiResponse> BoostGradeAsync(Guid id, [Body] BoostGradeRequest request, CancellationToken ct = default);
}