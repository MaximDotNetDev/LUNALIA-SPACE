namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IQuizQuestionRepository
{
    public Task<Guid> AddAsync(QuizQuestion question, CancellationToken cancellationToken = default);
    public Task<int> GetNextOrderIndexAsync(Guid quizId, CancellationToken cancellationToken = default);
    public Task<QuizQuestion?> GetByIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    public Task<QuizQuestion?> UpdateAsync(QuizQuestion question, CancellationToken cancellationToken = default);
    public Task<QuizQuestion?> DeleteAsync(Guid questionId, byte[] rowVersion, CancellationToken cancellationToken = default);
    public Task<bool> ReorderAsync(Guid quizId, string ordersJson, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<QuizQuestion> Items, int TotalCount)> GetPagedByQuizIdAsync(Guid quizId, int skip, int take, CancellationToken cancellationToken = default);
}