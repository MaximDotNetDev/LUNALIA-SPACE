namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface IGradeTypeRepository
{
    public Task<Guid> AddAsync(GradeType gradeType, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<GradeType?> GetByIdAsync(Guid gradeTypeId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<GradeType?> UpdateAsync(GradeType gradeType, CancellationToken cancellationToken = default);
    public Task<GradeType?> DeleteAsync(Guid gradeTypeId, CancellationToken cancellationToken = default);
    public Task<GradeType?> RestoreAsync(Guid gradeTypeId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<GradeType>> GetActiveAsync(CancellationToken cancellationToken = default);
    public Task<(IEnumerable<GradeType> Items, int TotalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}