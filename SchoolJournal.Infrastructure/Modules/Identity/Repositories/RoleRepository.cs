using Dapper;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Infrastructure.Common.Persistence;

namespace SchoolJournal.Infrastructure.Modules.Identity.Repositories;

public sealed class RoleRepository(SqlConnectionFactory connectionFactory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT RoleId, RoleName, Description 
            FROM [Identity].[Roles] 
            WHERE IsDeleted = 0 
            ORDER BY RoleName ASC;
            """;

        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Role>(new CommandDefinition(
            sql,
            cancellationToken: cancellationToken)).ConfigureAwait(false);
    }
}