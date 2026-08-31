namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IPedagogicalTitleRepository
{
    public Task<Guid> AddAsync(PedagogicalTitle pedagogicalTitle, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<PedagogicalTitle?> GetByIdAsync(Guid titleId, CancellationToken cancellationToken = default);
    public Task<PedagogicalTitle?> UpdateAsync(PedagogicalTitle pedagogicalTitle, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<PedagogicalTitle?> DeleteAsync(Guid titleId, CancellationToken cancellationToken = default);
    public Task<PedagogicalTitle?> RestoreAsync(Guid titleId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<PedagogicalTitle>> GetActiveAsync(CancellationToken cancellationToken = default);
    public Task<(IEnumerable<PedagogicalTitle> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<PedagogicalTitle> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}