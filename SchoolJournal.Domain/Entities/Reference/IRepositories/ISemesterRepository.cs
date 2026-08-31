namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface ISemesterRepository
{
    public Task<Guid> AddAsync(Semester semester, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingDatesAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
    public Task<Semester?> UpdateAsync(Semester semester, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingDatesExcludingIdAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Semester?> DeleteAsync(Guid semesterId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Semester?> GetByIdAsync(Guid semesterId, CancellationToken cancellationToken = default);
    public Task<Semester?> RestoreAsync(Guid semesterId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Semester> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Semester> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}