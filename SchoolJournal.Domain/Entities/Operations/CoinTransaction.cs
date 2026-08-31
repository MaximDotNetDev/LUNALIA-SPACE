namespace SchoolJournal.Domain.Entities.Operations;

public sealed record CoinTransaction
{
    public Guid TransactionId { get; init; }
    public Guid WalletId { get; init; }
    public int Amount { get; init; }
    public Guid ReferenceId { get; init; }
    public required string TransactionType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}