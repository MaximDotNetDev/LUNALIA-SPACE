using Refit;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Client.Core.Features.Operations.Lessons;

public interface ILessonApi
{
    [Get("/api/lessons/{id}")]
    public Task<IApiResponse<LessonResponse>> GetLessonByIdAsync(Guid id, CancellationToken ct = default);

    [Get("/api/lessons")]
    public Task<IApiResponse<IReadOnlyCollection<LessonResponse>>> GetLessonsByAssignmentAsync([Query] Guid assignmentId, CancellationToken ct = default);

    [Get("/api/lessons/schedule")]
    public Task<IApiResponse<IReadOnlyCollection<LessonResponse>>> GetScheduleAsync([Query] DateTimeOffset startDate, [Query] DateTimeOffset endDate, [Query] Guid semesterId, CancellationToken ct = default);

    [Get("/api/lessons/room/{roomId}/occupancy")]
    public Task<IApiResponse<IReadOnlyCollection<LessonResponse>>> GetRoomOccupancyAsync(Guid roomId, [Query] DateTimeOffset lessonDate, [Query] Guid? periodId, CancellationToken ct = default);

    [Post("/api/lessons")]
    public Task<IApiResponse<object>> CreateLessonAsync([Body] CreateLessonRequest request, CancellationToken ct = default);

    [Put("/api/lessons/{id}/topic-and-homework")]
    public Task<IApiResponse> UpdateTopicAndHomeworkAsync(Guid id, [Body] UpdateLessonTopicAndHomeworkRequest request, CancellationToken ct = default);

    [Patch("/api/lessons/{id}/reschedule")]
    public Task<IApiResponse> RescheduleLessonAsync(Guid id, [Body] RescheduleLessonRequest request, CancellationToken ct = default);

    [Delete("/api/lessons/{id}")]
    public Task<IApiResponse> DeleteLessonAsync(Guid id, [Body] DeleteLessonRequest request, CancellationToken ct = default);
}