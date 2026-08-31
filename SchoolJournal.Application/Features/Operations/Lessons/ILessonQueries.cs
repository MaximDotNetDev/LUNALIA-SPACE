using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons;

public interface ILessonQueries
{
    public Task<bool> CheckReadAccessAsync(Guid lessonId, Guid userId, string role, CancellationToken cancellationToken = default);
    public Task<LessonResponse?> GetDetailedByIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task<bool> CheckAssignmentReadAccessAsync(Guid assignmentId, Guid userId, string role, CancellationToken cancellationToken = default);
    public Task<IEnumerable<LessonResponse>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<LessonResponse>> GetScheduleAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid semesterId, Guid userId, string role, CancellationToken cancellationToken = default);
    public Task<IEnumerable<LessonResponse>> GetRoomOccupancyAsync(Guid roomId, DateTimeOffset lessonDate, Guid? periodId, Guid userId, string role, CancellationToken cancellationToken = default);

}