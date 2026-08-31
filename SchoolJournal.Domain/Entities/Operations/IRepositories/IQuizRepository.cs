namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IQuizRepository
{
    public Task<Guid> AddAsync(Quiz quiz, CancellationToken cancellationToken = default);
    public Task<Guid> AddGeneratedQuizAsync(Quiz quiz, IEnumerable<QuizQuestion> questions, CancellationToken cancellationToken = default);
    public Task<Quiz?> GetByIdAsync(Guid quizId, CancellationToken cancellationToken = default);
    public Task<(Quiz? Quiz, IEnumerable<QuizQuestion> Questions)> GetWithQuestionsByIdAsync(Guid quizId, CancellationToken cancellationToken = default); public Task<bool> TeacherExistsAsync(Guid teacherId, CancellationToken cancellationToken = default);
    public Task<bool> SubjectExistsAsync(Guid subjectId, CancellationToken cancellationToken = default);
    public Task<Quiz?> UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
    public Task<Quiz?> UpdateWithQuestionsAsync(Quiz quiz, IEnumerable<QuizQuestion> questions, CancellationToken cancellationToken = default);
    public Task<Quiz?> DeleteAsync(Guid quizId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedByTeacherIdAsync(Guid teacherId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedBySubjectIdAsync(Guid subjectId, int skip, int take, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<Quiz> Items, int TotalCount)> GetPagedAsync(string? searchTerm, int skip, int take, CancellationToken cancellationToken = default);
}