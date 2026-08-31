namespace SchoolJournal.Domain.Entities.Operations;

public sealed record Wallet
{
    public Guid WalletId { get; init; }
    public Guid StudentId { get; init; }
    public Guid SubjectId { get; init; }
    public int Balance { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}