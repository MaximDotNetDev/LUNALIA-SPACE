using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace SchoolJournal.IntegrationTests;

[Collection("Database test group")]
public abstract class DatabaseTestBase(MsSqlFixture fixture) : IAsyncDisposable
{
    protected SqlConnection DbConnection { get; } = new(fixture?.ConnectionString
        ?? throw new ArgumentNullException(nameof(fixture)));

    static DatabaseTestBase() => DefaultTypeMap.MatchNamesWithUnderscores = true;

    public async ValueTask DisposeAsync()
    {
        await DbConnection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected async ValueTask AssertSqlEmptyAsync(string sql, object? param = null)
    {
        var result = await DbConnection.QueryAsync<string>(sql, param);
        if (result.Any())
        {
            var details = string.Join(", ", result);
            Assert.Fail($"Found architectural violations: {details}");
        }
    }
}