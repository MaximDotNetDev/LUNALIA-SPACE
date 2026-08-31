namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IGradeRepository
{
    public Task<Guid> AddAsync(Grade grade, CancellationToken cancellationToken = default);
    public Task<Grade?> GetByIdAsync(Guid gradeId, CancellationToken cancellationToken = default);
    public Task<Grade?> UpdateAsync(Grade grade, CancellationToken cancellationToken = default);
    public Task<Grade?> DeleteAsync(Guid gradeId, Guid updatedByUserId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Grade>> GetByLessonIdAsync(Guid lessonId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Grade>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}