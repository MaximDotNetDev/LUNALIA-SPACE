namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface ITeachingAssignmentRepository
{
    public Task<Guid> AddAsync(TeachingAssignment assignment, CancellationToken cancellationToken = default);
    public Task<bool> ExistsAsync(Guid teacherId, Guid subjectId, Guid classId, Guid? subgroupId, CancellationToken cancellationToken = default);
    public Task<TeachingAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<bool> ExistsExcludingIdAsync(Guid teacherId, Guid subjectId, Guid classId, Guid? subgroupId, Guid excludeId, CancellationToken cancellationToken = default);
    public Task<TeachingAssignment?> UpdateAsync(TeachingAssignment assignment, CancellationToken cancellationToken = default);
    public Task<TeachingAssignment?> ToggleStatusAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<TeachingAssignment?> DeleteAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedByClassIdAsync(Guid classId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<TeachingAssignment> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default);
}