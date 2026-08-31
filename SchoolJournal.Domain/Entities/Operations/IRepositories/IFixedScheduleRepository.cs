using SchoolJournal.Domain.Entities.Operations.Models;

namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IFixedScheduleRepository
{
    public Task<Guid> AddAsync(FixedSchedule schedule, CancellationToken cancellationToken = default);
    public Task<FixedScheduleReadModel?> GetByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingRoomAsync(int dayOfWeek, Guid periodId, Guid roomId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingAssignmentAsync(int dayOfWeek, Guid periodId, Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<FixedSchedule?> UpdateAsync(FixedSchedule schedule, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingRoomExcludingIdAsync(int dayOfWeek, Guid periodId, Guid roomId, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingAssignmentExcludingIdAsync(int dayOfWeek, Guid periodId, Guid assignmentId, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<FixedSchedule?> DeleteAsync(Guid scheduleId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<IEnumerable<FixedScheduleReadModel>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<FixedScheduleReadModel>> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<FixedScheduleReadModel>> GetByDayAsync(int dayOfWeek, CancellationToken cancellationToken = default);
}