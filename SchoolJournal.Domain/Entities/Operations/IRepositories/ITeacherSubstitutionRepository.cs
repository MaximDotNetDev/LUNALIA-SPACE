using SchoolJournal.Domain.Entities.Operations.Models;

namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface ITeacherSubstitutionRepository
{
    public Task<Guid> AddAsync(TeacherSubstitution substitution, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingForAssignmentAsync(Guid assignmentId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingForTeacherAsync(Guid substituteTeacherId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
    public Task<TeacherSubstitutionDetailed?> GetByIdAsync(Guid substitutionId, CancellationToken cancellationToken = default);
    public Task<TeacherSubstitution?> UpdateAsync(TeacherSubstitution substitution, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingForAssignmentExcludingIdAsync(Guid assignmentId, DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeSubstitutionId, CancellationToken cancellationToken = default);
    public Task<bool> HasOverlappingForTeacherExcludingIdAsync(Guid substituteTeacherId, DateTimeOffset startDate, DateTimeOffset endDate, Guid excludeSubstitutionId, CancellationToken cancellationToken = default);
    public Task<TeacherSubstitution?> DeleteAsync(Guid substitutionId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<IEnumerable<TeacherSubstitutionDetailed>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<TeacherSubstitutionDetailed>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<TeacherSubstitutionDetailed>> GetActiveAsync(CancellationToken cancellationToken = default);
}