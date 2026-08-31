using Dapper;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Identity.Repositories;

public sealed class UserRepository(SqlConnectionFactory connectionFactory) : IUserRepository
{
    private const string ConnectionName = "IdentityConnection";

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection(ConnectionName);
        const string sql = """
            SELECT 
                u.UserId, u.Login, u.Email, u.PasswordHash, u.RoleId, 
                u.LastLoginUtc, u.FailedLoginAttempts, u.LockoutEndUtc, 
                u.IsActive, u.IsDeleted, u.CreatedAt, u.UpdatedAt, u.RowVersion,
                r.RoleName AS Role
            FROM [Identity].[Users] u
            INNER JOIN [Identity].[Roles] r ON u.RoleId = r.RoleId
            WHERE u.UserId = @UserId AND u.IsDeleted = 0
            """;

        var command = new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(command).ConfigureAwait(false);
    }

    public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection(ConnectionName);

        const string sql = """
            SELECT 
                u.UserId, u.Login, u.Email, u.PasswordHash, u.RoleId, 
                u.LastLoginUtc, u.FailedLoginAttempts, u.LockoutEndUtc, 
                u.IsActive, u.IsDeleted, u.CreatedAt, u.UpdatedAt, u.RowVersion,
                r.RoleName AS Role
            FROM [Identity].[Users] u
            INNER JOIN [Identity].[Roles] r ON u.RoleId = r.RoleId
            WHERE u.Login = @Login AND u.IsDeleted = 0
            """;

        var command = new CommandDefinition(
            sql,
            new { Login = login },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(command).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        using var connection = connectionFactory.CreateConnection(ConnectionName);

        const string sql = """
            UPDATE [Identity].[Users]
            SET 
                FailedLoginAttempts = @FailedLoginAttempts,
                LockoutEndUtc = @LockoutEndUtc,
                LastLoginUtc = @LastLoginUtc,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId AND RowVersion = @RowVersion
            """;

        var command = new CommandDefinition(
                    sql,
                    new
                    {
                        user.FailedLoginAttempts,
                        user.LockoutEndUtc,
                        user.LastLoginUtc,
                        user.UserId,
                        RowVersion = user.RowVersion?.ToArray()
                    },
                    cancellationToken: cancellationToken);

        int affectedRows = await connection.ExecuteAsync(command).ConfigureAwait(false);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Concurrency conflict: The user record was modified by another process.");
        }
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        using var connection = connectionFactory.CreateConnection(ConnectionName);


        // 2. Безпечно вставляємо користувача
        const string insertSql = """
            INSERT INTO [Identity].[Users] (
                UserId, Login, Email, PasswordHash, RoleId, 
                FailedLoginAttempts, IsActive, IsDeleted, CreatedAt
            )
            VALUES (
                @UserId, @Login, @Email, @PasswordHash, @RoleId,
                @FailedLoginAttempts, @IsActive, @IsDeleted, @CreatedAt
            )
            """;

        var command = new CommandDefinition(
            insertSql,
            new
            {
                user.UserId,
                user.Login,
                user.Email,
                user.PasswordHash,
                user.RoleId,
                user.FailedLoginAttempts,
                user.IsActive,
                user.IsDeleted,
                user.CreatedAt
            },
            cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }

    public async Task UpdateCredentialsAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        using var connection = connectionFactory.CreateConnection(ConnectionName);

        const string sql = """
            UPDATE [Identity].[Users]
            SET 
                Login = @Login,
                PasswordHash = @PasswordHash,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId AND RowVersion = @RowVersion
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                user.Login,
                user.PasswordHash,
                user.UserId,
                RowVersion = user.RowVersion.ToArray()
            },
            cancellationToken: cancellationToken);

        int affectedRows = await connection.ExecuteAsync(command).ConfigureAwait(false);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException("Конфлікт оновлення: запис користувача був змінений іншим процесом.");
        }
    }
}