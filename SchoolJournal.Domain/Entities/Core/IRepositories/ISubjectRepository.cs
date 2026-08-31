using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface ISubjectRepository
{
    public Task<Guid> AddAsync(Subject subject, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    public Task<Subject?> GetByIdAsync(Guid subjectId, CancellationToken cancellationToken = default);
    public Task<Subject?> UpdateAsync(Subject subject, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Subject?> DeleteAsync(Guid subjectId, CancellationToken cancellationToken = default);
    public Task<Subject?> RestoreAsync(Guid subjectId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Subject> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, string? searchTerm = null, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Subject> Items, int TotalCount)> GetDeletedPagedAsync(int skip, int take, string? searchTerm = null, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
}