using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Teachers;

namespace SchoolJournal.Client.Core.Features.Core.Teachers;

public interface ITeacherApi
{
    [Get("/api/teachers")]
    public Task<IApiResponse<PagedResponse<TeacherListItemResponse>>> GetTeachersAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? searchTerm = null,
        [Query] Guid? positionId = null,
        [Query] bool? isActive = null,
        CancellationToken ct = default);

    [Get("/api/teachers/{id}")]
    public Task<IApiResponse<TeacherResponse>> GetTeacherByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/teachers")]
    public Task<IApiResponse<object>> CreateTeacherAsync([Body] CreateTeacherRequest request, CancellationToken ct = default);

    [Put("/api/teachers/{id}")]
    public Task<IApiResponse> UpdateTeacherProfileAsync(Guid id, [Body] UpdateTeacherProfileRequest request, CancellationToken ct = default);

    [Put("/api/teachers/{id}/academic-info")]
    public Task<IApiResponse> UpdateTeacherAcademicInfoAsync(Guid id, [Body] UpdateTeacherAcademicInfoRequest request, CancellationToken ct = default);

    [Patch("/api/teachers/{id}/status")]
    public Task<IApiResponse> ToggleTeacherStatusAsync(Guid id, [Body] ToggleTeacherStatusRequest request, CancellationToken ct = default);

    [Delete("/api/teachers/{id}")]
    public Task<IApiResponse> DeleteTeacherAsync(Guid id, [Body] DeleteTeacherRequest request, CancellationToken ct = default);

    [Get("/api/teachers/by-user/{userId}")]
    public Task<IApiResponse<TeacherResponse>> GetTeacherByUserIdAsync(Guid userId, CancellationToken ct = default);

    [Patch("/api/teachers/{id}/user")]
    public Task<IApiResponse> AssignTeacherUserAsync(Guid id, [Body] AssignTeacherUserRequest request, CancellationToken ct = default);
}