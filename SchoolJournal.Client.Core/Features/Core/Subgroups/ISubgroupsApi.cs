using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;

namespace SchoolJournal.Client.Core.Features.Core.Subgroups;

public interface ISubgroupsApi
{
    [Get("/api/subgroups")]
    public Task<IApiResponse<PagedResponse<SubgroupResponse>>> GetSubgroupsListAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/subgroups/{id}")]
    public Task<IApiResponse<SubgroupResponse>> GetSubgroupByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/subgroups")]
    public Task<IApiResponse<object>> CreateSubgroupAsync([Body] CreateSubgroupRequest request, CancellationToken ct = default);

    [Put("/api/subgroups/{id}")]
    public Task<IApiResponse> UpdateSubgroupAsync(Guid id, [Body] UpdateSubgroupRequest request, CancellationToken ct = default);

    [Delete("/api/subgroups/{id}")]
    public Task<IApiResponse> DeleteSubgroupAsync(Guid id, [Body] DeleteSubgroupRequest request, CancellationToken ct = default);

    [Post("/api/subgroups/{id}/restore")]
    public Task<IApiResponse> RestoreSubgroupAsync(Guid id, [Body] RestoreSubgroupRequest request, CancellationToken ct = default);

    [Get("/api/classes/{classId}/subgroups")]
    public Task<IApiResponse<IReadOnlyCollection<SubgroupResponse>>> GetSubgroupsByClassAsync(Guid classId, CancellationToken ct = default);

    [Get("/api/classes/{classId}/subjects/{subjectId}/subgroups")]
    public Task<IApiResponse<IReadOnlyCollection<SubgroupResponse>>> GetSubgroupsBySubjectAsync(Guid classId, Guid subjectId, CancellationToken ct = default);
}