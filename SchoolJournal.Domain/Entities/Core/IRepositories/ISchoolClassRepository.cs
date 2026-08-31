using SchoolJournal.Domain.Entities.Core.Models;

namespace SchoolJournal.Domain.Entities.Core.IRepositories;

public interface ISchoolClassRepository
{
    public Task<Guid> AddAsync(SchoolClass schoolClass, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAndYearAsync(string className, string academicYear, CancellationToken cancellationToken = default);
    public Task<bool> TeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken = default);
    public Task<SchoolClass?> GetByIdAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<SchoolClass?> UpdateAsync(SchoolClass schoolClass, CancellationToken cancellationToken = default);
    public Task<bool> ExistsByNameAndYearExcludingIdAsync(string className, string academicYear, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<SchoolClass?> UpdateHomeroomTeacherAsync(Guid classId, Guid newTeacherId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<SchoolClass?> ChangeActiveStatusAsync(Guid classId, bool isActive, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<SchoolClass?> DeleteAsync(Guid classId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<SchoolClassDetailsModel?> GetDetailsByIdAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<SchoolClassItemModel> Items, int TotalCount)> GetActivePagedAsync(int skip, int take, string? academicYear, CancellationToken cancellationToken = default);
    public Task<IEnumerable<SchoolClassItemModel>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);

}