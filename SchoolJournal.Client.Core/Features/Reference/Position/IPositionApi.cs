using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Positions;

namespace SchoolJournal.Client.Core.Features.Reference.Position;

public interface IPositionApi
{
    [Get("/api/positions")]
    public Task<IApiResponse<PagedResponse<PositionResponse>>> GetPositionsAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/positions/{id}")]
    public Task<IApiResponse<PositionResponse>> GetPositionByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/positions")]
    public Task<IApiResponse<object>> CreatePositionAsync([Body] CreatePositionRequest request, CancellationToken ct = default);

    [Put("/api/positions/{id}")]
    public Task<IApiResponse> UpdatePositionAsync(Guid id, [Body] UpdatePositionRequest request, CancellationToken ct = default);

    [Delete("/api/positions/{id}")]
    public Task<IApiResponse> DeletePositionAsync(Guid id, CancellationToken ct = default);
}