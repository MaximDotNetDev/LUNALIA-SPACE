namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IQualificationRepository
{
    public Task<Guid> AddAsync(Qualification qualification, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<Qualification?> GetByIdAsync(Guid qualificationId, CancellationToken cancellationToken = default);
    public Task<Qualification?> UpdateAsync(Qualification qualification, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Qualification?> DeleteAsync(Guid qualificationId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Qualification?> RestoreAsync(Guid qualificationId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Qualification> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Qualification> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}