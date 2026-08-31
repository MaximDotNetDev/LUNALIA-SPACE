using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface IStudentSubgroupRepository
{
    public Task<Guid> AddAsync(StudentSubgroup studentSubgroup, CancellationToken cancellationToken = default);
    public Task<bool> ExistsActiveAsync(Guid studentId, Guid subgroupId, CancellationToken cancellationToken = default);
    public Task<StudentSubgroup?> GetByIdAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default);
    public Task<StudentSubgroup?> DeleteAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default);
    public Task<StudentSubgroup?> UpdateSubgroupIdAsync(Guid studentSubgroupId, Guid newSubgroupId, CancellationToken cancellationToken = default);
    public Task<StudentSubgroup?> RestoreAsync(Guid studentSubgroupId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<SubgroupStudentItem>> GetStudentsBySubgroupIdAsync(Guid subgroupId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<StudentSubgroupItem>> GetSubgroupsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<AvailableStudentItem>> GetAvailableStudentsForSubgroupIdAsync(Guid subgroupId, CancellationToken cancellationToken = default);
}