using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface IStudentParentRepository
{
    public Task<Guid> AddAsync(StudentParent studentParent, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(Guid studentId, Guid parentId, CancellationToken cancellationToken = default);
    public Task<StudentParent?> GetByIdAsync(Guid studentParentId, CancellationToken cancellationToken = default);
    public Task<StudentParent?> UpdateRoleAsync(Guid studentParentId, string? role, CancellationToken cancellationToken = default);
    public Task<StudentParent?> DeleteAsync(Guid studentParentId, CancellationToken cancellationToken = default);
    public Task<StudentParent?> RestoreAsync(Guid studentParentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<StudentParentDetail>> GetParentsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<ParentStudentDetail>> GetStudentsByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);

}