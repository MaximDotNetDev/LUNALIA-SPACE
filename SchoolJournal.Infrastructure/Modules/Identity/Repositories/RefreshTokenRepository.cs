using Dapper;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Identity.Repositories;

public sealed class RefreshTokenRepository(SqlConnectionFactory connectionFactory) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        using var connection = connectionFactory.CreateConnection("IdentityConnection");

        const string sql = """
            INSERT INTO [Identity].[RefreshTokens] (
                TokenId, UserId, TokenHash, ExpiresAt, CreatedByIp, 
                DeviceIdentifier, Revoked, IsDeleted, CreatedAt
            )
            VALUES (
                @TokenId, @UserId, @TokenHash, @ExpiresAt, @CreatedByIp, 
                @DeviceIdentifier, @Revoked, @IsDeleted, @CreatedAt
            )
            """;

        var command = new CommandDefinition(
                    sql,
                    new
                    {
                        token.TokenId,
                        token.UserId,
                        token.TokenHash,
                        token.ExpiresAt,
                        token.CreatedByIp,
                        token.DeviceIdentifier,
                        token.Revoked,
                        token.IsDeleted,
                        token.CreatedAt
                    },
                    cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        using var connection = connectionFactory.CreateConnection("IdentityConnection");

        const string sql = """
            SELECT TokenId, UserId, TokenHash, ExpiresAt, CreatedByIp, 
                   DeviceIdentifier, Revoked, RevokedAt, ReplacedByTokenHash, 
                   IsDeleted, CreatedAt, UpdatedAt, RowVersion
            FROM [Identity].[RefreshTokens]
            WHERE TokenHash = @TokenHash AND IsDeleted = 0
            """;

        var command = new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(command).ConfigureAwait(false);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        using var connection = connectionFactory.CreateConnection("IdentityConnection");

        const string sql = """
            UPDATE [Identity].[RefreshTokens]
            SET Revoked = @Revoked,
                RevokedAt = @RevokedAt,
                ReplacedByTokenHash = @ReplacedByTokenHash,
                UpdatedAt = SYSDATETIMEOFFSET()
            WHERE TokenId = @TokenId AND RowVersion = @RowVersion AND IsDeleted = 0
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                token.Revoked,
                token.RevokedAt,
                token.ReplacedByTokenHash,
                token.TokenId,
                RowVersion = token.RowVersion?.ToArray()
            },
            cancellationToken: cancellationToken);

        int affectedRows = await connection.ExecuteAsync(command).ConfigureAwait(false);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Concurrency conflict: The refresh token was modified or deleted by another process.");
        }
    }
}