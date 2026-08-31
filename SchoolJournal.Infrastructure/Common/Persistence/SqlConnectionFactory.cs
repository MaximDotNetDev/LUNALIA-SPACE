using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SchoolJournal.Infrastructure.Common.Persistence;

public sealed class SqlConnectionFactory(IConfiguration configuration)
{
    public IDbConnection CreateConnection(string connectionName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

        return new SqlConnection(connectionString);
    }
}