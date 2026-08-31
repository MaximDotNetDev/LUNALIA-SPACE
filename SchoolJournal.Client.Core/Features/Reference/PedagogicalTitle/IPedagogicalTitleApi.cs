using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;

namespace SchoolJournal.Client.Core.Features.Reference.PedagogicalTitle;

public interface IPedagogicalTitleApi
{
    [Get("/api/pedagogical-titles")]
    public Task<IApiResponse<PagedResponse<PedagogicalTitleResponse>>> GetActivePagedAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/pedagogical-titles/archive")]
    public Task<IApiResponse<PagedResponse<PedagogicalTitleResponse>>> GetDeletedPagedAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/pedagogical-titles/{id}")]
    public Task<IApiResponse<PedagogicalTitleResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/pedagogical-titles/active")]
    public Task<IApiResponse<IEnumerable<PedagogicalTitleResponse>>> GetActiveListAsync(CancellationToken ct = default);

    [Post("/api/pedagogical-titles")]
    public Task<IApiResponse<object>> CreateAsync([Body] CreatePedagogicalTitleRequest request, CancellationToken ct = default);

    [Put("/api/pedagogical-titles/{id}")]
    public Task<IApiResponse> UpdateAsync(Guid id, [Body] UpdatePedagogicalTitleRequest request, CancellationToken ct = default);

    [Delete("/api/pedagogical-titles/{id}")]
    public Task<IApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);

    [Post("/api/pedagogical-titles/{id}/restore")]
    public Task<IApiResponse> RestoreAsync(Guid id, CancellationToken ct = default);
}