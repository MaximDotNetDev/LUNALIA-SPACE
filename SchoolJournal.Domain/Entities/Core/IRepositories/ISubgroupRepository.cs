namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface ISubgroupRepository
{
    public Task<Guid> AddAsync(Subgroup subgroup, CancellationToken cancellationToken = default);
    public Task<Subgroup?> GetByIdAsync(Guid subgroupId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameInClassAsync(Guid classId, string name, CancellationToken cancellationToken = default);
    public Task<bool> ClassExistsAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken = default);
    public Task<Subgroup?> UpdateAsync(Subgroup subgroup, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameInClassExcludingIdAsync(Guid classId, string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Subgroup?> DeleteAsync(Guid subgroupId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<Subgroup?> RestoreAsync(Guid subgroupId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Subgroup>> GetByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Subgroup>> GetBySubjectAndClassIdAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<SchoolJournal.Domain.Entities.Core.Models.SubgroupListItem> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}