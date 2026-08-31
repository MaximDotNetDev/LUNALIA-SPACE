using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Client.Core.Features.Operations.TeachingAssignments;

public interface ITeachingAssignmentApi
{
    [Get("/api/teaching-assignments/{id}")]
    public Task<IApiResponse<TeachingAssignmentResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/teachers/{teacherId}/assignments")]
    public Task<IApiResponse<PagedResponse<TeachingAssignmentResponse>>> GetByTeacherIdAsync(
        Guid teacherId, [Query] int pageNumber, [Query] int pageSize, CancellationToken ct = default);

    [Get("/api/classes/{classId}/assignments")]
    public Task<IApiResponse<PagedResponse<TeachingAssignmentResponse>>> GetByClassIdAsync(
        Guid classId, [Query] int pageNumber, [Query] int pageSize, CancellationToken ct = default);

    [Get("/api/subjects/{subjectId}/assignments")]
    public Task<IApiResponse<PagedResponse<TeachingAssignmentResponse>>> GetBySubjectIdAsync(
        Guid subjectId, [Query] int pageNumber, [Query] int pageSize, CancellationToken ct = default);

    [Post("/api/teaching-assignments")]
    public Task<IApiResponse<object>> CreateAsync([Body] CreateTeachingAssignmentRequest request, CancellationToken ct = default);

    [Put("/api/teaching-assignments/{id}")]
    public Task<IApiResponse> UpdateAsync(Guid id, [Body] UpdateTeachingAssignmentRequest request, CancellationToken ct = default);

    [Patch("/api/teaching-assignments/{id}/toggle-status")]
    public Task<IApiResponse> ToggleStatusAsync(Guid id, [Body] ToggleTeachingAssignmentStatusRequest request, CancellationToken ct = default);

    [Delete("/api/teaching-assignments/{id}")]
    public Task<IApiResponse> DeleteAsync(Guid id, [Body] DeleteTeachingAssignmentRequest request, CancellationToken ct = default);
}