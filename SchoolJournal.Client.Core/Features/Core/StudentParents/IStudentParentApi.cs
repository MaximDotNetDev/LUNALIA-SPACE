using Refit;
using SchoolJournal.Contracts.DTOs.Core.StudentParents;

namespace SchoolJournal.Client.Core.Features.Core.StudentParents;

public interface IStudentParentApi
{
    [Post("/api/student-parents")]
    public Task<IApiResponse<object>> AssignParentToStudentAsync([Body] AssignParentToStudentRequest request, CancellationToken ct = default);

    [Put("/api/student-parents/{id}/role")]
    public Task<IApiResponse> UpdateStudentParentRoleAsync(Guid id, [Body] UpdateStudentParentRoleRequest request, CancellationToken ct = default);

    [Delete("/api/student-parents/{id}")]
    public Task<IApiResponse> RemoveParentFromStudentAsync(Guid id, CancellationToken ct = default);

    [Post("/api/student-parents/{id}/restore")]
    public Task<IApiResponse> RestoreStudentParentAsync(Guid id, CancellationToken ct = default);

    [Get("/api/students/{studentId}/parents")]
    public Task<IApiResponse<IEnumerable<StudentParentDetailResponse>>> GetParentsByStudentIdAsync(Guid studentId, CancellationToken ct = default);

    [Get("/api/parents/{parentId}/students")]
    public Task<IApiResponse<IEnumerable<ParentStudentDetailResponse>>> GetStudentsByParentIdAsync(Guid parentId, CancellationToken ct = default);

    [Get("/api/student-parents/{id}")]
    public Task<IApiResponse<StudentParentResponse>> GetStudentParentByIdAsync(Guid id, CancellationToken ct = default);
}