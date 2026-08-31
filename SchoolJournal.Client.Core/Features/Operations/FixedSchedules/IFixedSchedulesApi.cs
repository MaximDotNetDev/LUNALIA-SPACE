using Refit;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

namespace SchoolJournal.Client.Core.Features.Operations.FixedSchedules;

public interface IFixedSchedulesApi
{
    [Get("/api/fixed-schedules/{id}")]
    public Task<IApiResponse<FixedScheduleResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/fixed-schedules/assignment/{assignmentId}")]
    public Task<IApiResponse<IEnumerable<FixedScheduleResponse>>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken ct = default);

    [Get("/api/fixed-schedules/room/{roomId}")]
    public Task<IApiResponse<IEnumerable<FixedScheduleResponse>>> GetByRoomIdAsync(Guid roomId, CancellationToken ct = default);

    [Get("/api/fixed-schedules/day/{dayOfWeek}")]
    public Task<IApiResponse<IEnumerable<FixedScheduleResponse>>> GetByDayAsync(int dayOfWeek, CancellationToken ct = default);

    [Post("/api/fixed-schedules")]
    public Task<IApiResponse<object>> CreateAsync([Body] CreateFixedScheduleRequest request, CancellationToken ct = default);

    [Put("/api/fixed-schedules/{id}")]
    public Task<IApiResponse> UpdateAsync(Guid id, [Body] UpdateFixedScheduleRequest request, CancellationToken ct = default);

    [Delete("/api/fixed-schedules/{id}")]
    public Task<IApiResponse> DeleteAsync(Guid id, [Body] DeleteFixedScheduleRequest request, CancellationToken ct = default);
}