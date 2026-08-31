namespace SchoolJournal.Domain.Entities.Reference.IRepositories;

public interface ILessonTypeRepository
{
    public Task<Guid> AddAsync(LessonType lessonType, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string typeName, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string typeName, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<LessonType?> GetByIdAsync(Guid lessonTypeId, CancellationToken cancellationToken = default);
    public Task<LessonType?> UpdateAsync(LessonType lessonType, CancellationToken cancellationToken = default);
    public Task<LessonType?> DeleteAsync(Guid lessonTypeId, CancellationToken cancellationToken = default);
    public Task<LessonType?> RestoreAsync(Guid lessonTypeId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<LessonType> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<LessonType> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
}