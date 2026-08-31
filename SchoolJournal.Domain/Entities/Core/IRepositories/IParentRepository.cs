namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface IParentRepository
{
    public Task<Guid> AddAsync(Parent parent, CancellationToken cancellationToken = default);
    public Task<Parent?> GetByIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    public Task<Parent?> UpdateAsync(Parent parent, CancellationToken cancellationToken = default);
    public Task<Parent?> DeleteAsync(Guid parentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Parent?> LinkToUserAsync(Guid parentId, Guid userId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Parent?> ToggleStatusAsync(Guid parentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Models.ParentListItemResult> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    public Task<Parent?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

}