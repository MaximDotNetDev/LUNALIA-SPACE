using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;

namespace SchoolJournal.Client.Core.Features.Reference.GradeType;

public interface IGradeTypeApi
{
    [Get("/api/gradetypes")]
    public Task<IApiResponse<IEnumerable<GradeTypeResponse>>> GetActiveGradeTypesAsync(CancellationToken ct = default);

    [Get("/api/gradetypes/archive")]
    public Task<IApiResponse<PagedResponse<GradeTypeResponse>>> GetDeletedGradeTypesArchiveAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/gradetypes/{id}")]
    public Task<IApiResponse<GradeTypeResponse>> GetGradeTypeByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/gradetypes")]
    public Task<IApiResponse<object>> CreateGradeTypeAsync([Body] CreateGradeTypeRequest request, CancellationToken ct = default);

    [Put("/api/gradetypes/{id}")]
    public Task<IApiResponse> UpdateGradeTypeAsync(Guid id, [Body] UpdateGradeTypeRequest request, CancellationToken ct = default);

    [Delete("/api/gradetypes/{id}")]
    public Task<IApiResponse> DeleteGradeTypeAsync(Guid id, CancellationToken ct = default);

    [Post("/api/gradetypes/{id}/restore")]
    public Task<IApiResponse> RestoreGradeTypeAsync(Guid id, CancellationToken ct = default);
}