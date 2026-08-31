using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;

namespace SchoolJournal.Client.Core.Features.Reference.Qualification;

public interface IQualificationApi
{
    [Get("/api/qualifications")]
    public Task<IApiResponse<PagedResponse<QualificationResponse>>> GetActiveQualificationsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/qualifications/archive")]
    public Task<IApiResponse<PagedResponse<QualificationResponse>>> GetDeletedQualificationsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/qualifications/{id}")]
    public Task<IApiResponse<QualificationResponse>> GetQualificationByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/qualifications")]
    public Task<IApiResponse<object>> CreateQualificationAsync([Body] CreateQualificationRequest request, CancellationToken ct = default);

    [Put("/api/qualifications/{id}")]
    public Task<IApiResponse> UpdateQualificationAsync(Guid id, [Body] UpdateQualificationRequest request, CancellationToken ct = default);

    [Delete("/api/qualifications/{id}")]
    public Task<IApiResponse> DeleteQualificationAsync(Guid id, [Body] DeleteQualificationRequest request, CancellationToken ct = default);

    [Post("/api/qualifications/{id}/restore")]
    public Task<IApiResponse> RestoreQualificationAsync(Guid id, [Body] RestoreQualificationRequest request, CancellationToken ct = default);
}