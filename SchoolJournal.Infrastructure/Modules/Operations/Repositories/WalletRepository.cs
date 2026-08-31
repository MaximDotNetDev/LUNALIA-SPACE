using System.Data;
using Dapper;
using SchoolJournal.Domain.Entities.Operations;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Operations.Repositories;

public sealed class WalletRepository(SqlConnectionFactory connectionFactory) : IWalletRepository
{
    public async Task<Wallet?> GetWalletAsync(Guid studentId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT * FROM [Operations].[Wallets]
            WHERE StudentId = @StudentId AND SubjectId = @SubjectId AND IsDeleted = 0;
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Wallet>(new CommandDefinition(
            sql, new { StudentId = studentId, SubjectId = subjectId }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<Guid> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wallet);
        const string sql = """
            INSERT INTO [Operations].[Wallets] (StudentId, SubjectId, Balance)
            OUTPUT INSERTED.WalletId
            VALUES (@StudentId, @SubjectId, @Balance);
            """;
        using var connection = connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            sql, new { wallet.StudentId, wallet.SubjectId, wallet.Balance }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task<bool> UpdateBalanceAsync(Guid walletId, int newBalance, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [Operations].[Wallets]
            SET Balance = @Balance, UpdatedAt = GETUTCDATE()
            WHERE WalletId = @WalletId AND RowVersion = @RowVersion AND IsDeleted = 0;
            """;
        using var connection = connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(
            sql, new { Balance = newBalance, WalletId = walletId, RowVersion = rowVersion }, cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rowsAffected > 0;
    }

    public async Task RecordTransactionAsync(CoinTransaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        const string sql = """
            INSERT INTO [Operations].[CoinTransactions] (WalletId, Amount, ReferenceId, TransactionType)
            VALUES (@WalletId, @Amount, @ReferenceId, @TransactionType);
            """;
        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(
            sql, new { transaction.WalletId, transaction.Amount, transaction.ReferenceId, transaction.TransactionType }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}