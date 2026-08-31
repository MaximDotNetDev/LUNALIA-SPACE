using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface ITeacherRepository
{
    public Task<Guid> AddAsync(Teacher teacher, CancellationToken cancellationToken = default);
    public Task<Teacher?> GetByIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByPhoneExcludingIdAsync(string phone, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<Teacher?> UpdateProfileAsync(Teacher teacher, CancellationToken cancellationToken = default);
    public Task<Teacher?> UpdateAcademicInfoAsync(Teacher teacher, CancellationToken cancellationToken = default);
    public Task<bool> IsUserAssignedToAnotherTeacherAsync(Guid userId, Guid excludeTeacherId, CancellationToken cancellationToken = default);
    public Task<Teacher?> AssignUserAsync(Teacher teacher, CancellationToken cancellationToken = default);
    public Task<Teacher?> ToggleStatusAsync(Teacher teacher, CancellationToken cancellationToken = default);
    public Task<Teacher?> DeleteAsync(Guid teacherId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<TeacherDetailsResult?> GetDetailsByIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeacherListItemResult> Items, int TotalCount)> GetPagedAsync(string? searchTerm, Guid? positionId, bool? isActive, int skip, int take, CancellationToken cancellationToken = default);
    public Task<TeacherDetailsResult?> GetDetailsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<TeacherWorkloadResult>> GetWorkloadSummaryAsync(bool onlyActive, CancellationToken cancellationToken = default);
}