namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IBellScheduleRepository
{
    public Task<Guid> AddAsync(BellSchedule schedule, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByLessonNumberAsync(int lessonNumber, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingTimeAsync(DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken = default);
    public Task<BellSchedule?> GetByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    public Task<BellSchedule?> UpdateAsync(BellSchedule schedule, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByLessonNumberExcludingIdAsync(int lessonNumber, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingTimeExcludingIdAsync(DateTimeOffset startTime, DateTimeOffset endTime, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<BellSchedule?> DeleteAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<BellSchedule> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}