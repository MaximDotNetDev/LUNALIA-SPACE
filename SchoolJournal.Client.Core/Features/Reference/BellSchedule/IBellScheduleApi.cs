using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

namespace SchoolJournal.Client.Core.Features.Reference.BellSchedule;

public interface IBellScheduleApi
{
    [Get("/api/bell-schedules")]
    public Task<IApiResponse<PagedResponse<BellScheduleResponse>>> GetActiveBellSchedulesAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        CancellationToken ct = default);

    [Get("/api/bell-schedules/{id}")]
    public Task<IApiResponse<BellScheduleResponse>> GetBellScheduleByIdAsync(Guid id, CancellationToken ct = default);

    [Post("/api/bell-schedules")]
    public Task<IApiResponse<object>> CreateBellScheduleAsync([Body] CreateBellScheduleRequest request, CancellationToken ct = default);

    [Put("/api/bell-schedules/{id}")]
    public Task<IApiResponse> UpdateBellScheduleAsync(Guid id, [Body] UpdateBellScheduleRequest request, CancellationToken ct = default);

    [Delete("/api/bell-schedules/{id}")]
    public Task<IApiResponse> DeleteBellScheduleAsync(Guid id, CancellationToken ct = default);
}