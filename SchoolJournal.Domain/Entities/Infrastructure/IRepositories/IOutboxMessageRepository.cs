namespace SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

public interface IOutboxMessageRepository
{
    public Task<Guid> AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    public Task<bool> UpdateStatusAsync(Guid id, DateTimeOffset? processedOnUtc, string? errorMessage = null, CancellationToken cancellationToken = default);
    public Task<int> DeleteProcessedOlderThanAsync(DateTimeOffset olderThanUtc, CancellationToken cancellationToken = default);
    public Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);
    public Task<(IEnumerable<OutboxMessage> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int take,
            string? type = null,
            bool? hasError = null,
            CancellationToken cancellationToken = default);
    public Task<OutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
