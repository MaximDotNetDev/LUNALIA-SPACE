namespace SchoolJournal.Domain.Entities.Operations.IRepositories;

public interface IWalletRepository
{
    /// <summary>
    /// Отримує гаманець за вектором (Студент + Предмет).
    /// </summary>
    public Task<Wallet?> GetWalletAsync(Guid studentId, Guid subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Створює новий гаманець, якщо його ще немає для цього предмета.
    /// </summary>
    public Task<Guid> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Атомарне оновлення балансу з перевіркою RowVersion (Optimistic Concurrency).
    /// </summary>
    public Task<bool> UpdateBalanceAsync(Guid walletId, int newBalance, byte[] rowVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Записує транзакцію в історію (Audit Trail).
    /// </summary>
    public Task RecordTransactionAsync(CoinTransaction transaction, CancellationToken cancellationToken = default);
}