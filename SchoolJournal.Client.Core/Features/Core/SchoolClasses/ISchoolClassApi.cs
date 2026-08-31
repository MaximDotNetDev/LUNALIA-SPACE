using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

namespace SchoolJournal.Client.Core.Features.Core.SchoolClasses;

public interface ISchoolClassApi
{
    [Get("/api/classes")]
    public Task<IApiResponse<PagedResponse<SchoolClassItemResponse>>> GetActiveClassesAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? academicYear = null,
        CancellationToken ct = default);

    [Get("/api/classes/{id}")]
    public Task<IApiResponse<SchoolClassResponse>> GetClassByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/classes")]
    public Task<IApiResponse<object>> CreateClassAsync([Body] CreateSchoolClassRequest request, CancellationToken ct = default);

    [Put("/api/classes/{id}")]
    public Task<IApiResponse> UpdateClassAsync(Guid id, [Body] UpdateSchoolClassRequest request, CancellationToken ct = default);

    [Patch("/api/classes/{id}/teacher")]
    public Task<IApiResponse> AssignTeacherAsync(Guid id, [Body] AssignHomeroomTeacherRequest request, CancellationToken ct = default);

    [Post("/api/classes/{id}/activate")]
    public Task<IApiResponse> ActivateClassAsync(Guid id, [Body] ChangeSchoolClassStatusRequest request, CancellationToken ct = default);

    [Post("/api/classes/{id}/deactivate")]
    public Task<IApiResponse> DeactivateClassAsync(Guid id, [Body] ChangeSchoolClassStatusRequest request, CancellationToken ct = default);

    [Delete("/api/classes/{id}")]
    public Task<IApiResponse> DeleteClassAsync(Guid id, [Body] DeleteSchoolClassRequest request, CancellationToken ct = default);

    [Get("/api/teachers/{teacherId}/classes")]
    public Task<IApiResponse<IReadOnlyCollection<SchoolClassItemResponse>>> GetClassesByTeacherAsync(Guid teacherId, CancellationToken ct = default);
}