namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IQuizAssignmentRepository
{
    public Task<Guid> AddAsync(QuizAssignment assignment, CancellationToken cancellationToken = default);
    public Task<QuizAssignment?> GetByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    public Task<QuizAssignment?> UpdateAsync(QuizAssignment assignment, CancellationToken cancellationToken = default);
    public Task<QuizAssignment?> DeleteAsync(Guid assignmentId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<bool> TeacherTeachesClassAsync(Guid teacherId, Guid classId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Models.QuizAssignmentDetailsResult>> GetActiveByClassIdAsync(Guid classId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Models.QuizAssignmentDetailsResult>> GetActiveByQuizIdAsync(Guid quizId, CancellationToken cancellationToken = default);
    public Task<Guid> GetSubjectIdByAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}