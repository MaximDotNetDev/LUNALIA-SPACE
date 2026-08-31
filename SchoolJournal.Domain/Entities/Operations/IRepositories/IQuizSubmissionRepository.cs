namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IQuizSubmissionRepository
{
    public Task<Guid> AddAsync(QuizSubmission submission, CancellationToken cancellationToken = default);
    public Task<bool> HasStudentSubmittedAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<Models.QuizSubmissionResult>> GetAssignmentSubmissionsAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}