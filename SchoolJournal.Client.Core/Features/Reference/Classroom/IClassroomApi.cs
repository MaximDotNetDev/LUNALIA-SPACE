using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;

namespace SchoolJournal.Client.Core.Features.Reference.Classroom;

public interface IClassroomApi
{
    [Get("/api/classrooms")]
    public Task<IApiResponse<PagedResponse<ClassroomResponse>>> GetActiveClassroomsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? searchTerm = null,
        CancellationToken ct = default);

    [Get("/api/classrooms/archive")]
    public Task<IApiResponse<PagedResponse<ClassroomResponse>>> GetDeletedClassroomsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? searchTerm = null,
        CancellationToken ct = default);

    [Get("/api/classrooms/{id}")]
    public Task<IApiResponse<ClassroomResponse>> GetClassroomByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/classrooms")]
    public Task<IApiResponse<object>> CreateClassroomAsync([Body] CreateClassroomRequest request, CancellationToken ct = default);

    [Put("/api/classrooms/{id}")]
    public Task<IApiResponse> UpdateClassroomAsync(Guid id, [Body] UpdateClassroomRequest request, CancellationToken ct = default);

    [Delete("/api/classrooms/{id}")]
    public Task<IApiResponse> DeleteClassroomAsync(Guid id, [Body] DeleteClassroomRequest request, CancellationToken ct = default);

    [Post("/api/classrooms/{id}/restore")]
    public Task<IApiResponse> RestoreClassroomAsync(Guid id, [Body] RestoreClassroomRequest request, CancellationToken ct = default);
}