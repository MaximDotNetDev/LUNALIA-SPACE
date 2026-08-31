using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Parents;

namespace SchoolJournal.Client.Core.Features.Core.Parents;

public interface IParentApi
{
    [Get("/api/parents")]
    public Task<IApiResponse<PagedResponse<ParentResponse>>> GetParentsPagedAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/parents/my-profile")]
    public Task<IApiResponse<ParentResponse>> GetMyProfileAsync(CancellationToken ct = default);

    [Get("/api/parents/{id}")]
    public Task<IApiResponse<ParentResponse>> GetParentByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/parents")]
    public Task<IApiResponse<object>> CreateParentAsync([Body] CreateParentRequest request, CancellationToken ct = default);

    [Put("/api/parents/{id}")]
    public Task<IApiResponse> UpdateParentAsync(Guid id, [Body] UpdateParentRequest request, CancellationToken ct = default);

    [Delete("/api/parents/{id}")]
    public Task<IApiResponse> DeleteParentAsync(Guid id, [Body] DeleteParentRequest request, CancellationToken ct = default);

    [Post("/api/parents/{id}/toggle-status")]
    public Task<IApiResponse> ToggleParentStatusAsync(Guid id, [Body] ToggleParentStatusRequest request, CancellationToken ct = default);

    [Post("/api/parents/{id}/link-user")]
    public Task<IApiResponse> LinkParentToUserAsync(Guid id, [Body] LinkParentToUserRequest request, CancellationToken ct = default);
}