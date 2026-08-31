namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IPositionRepository
{
    public Task<bool> ExistsByNameAsync(string positionName, CancellationToken cancellationToken = default);
    public Task<Guid> AddAsync(Position position, CancellationToken cancellationToken = default);
    public Task<Position?> GetByIdAsync(Guid positionId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string positionName, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Position?> UpdateAsync(Position position, CancellationToken cancellationToken = default);
    public Task<Position?> DeleteAsync(Guid positionId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Position> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}