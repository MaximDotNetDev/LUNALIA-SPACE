using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseNamingTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task TableNamesShouldFollowPascalCase()
    {
        string sql = $@"
        SELECT t.name FROM sys.tables t 
        WHERE {DatabaseNamingConventions.TablePascalCaseViolationFilter}
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ColumnNamesShouldFollowPascalCase()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name 
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE {DatabaseNamingConventions.ColumnPascalCaseViolationFilter}
          AND c.name NOT LIKE 'ledger_%'
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";
        
        await AssertSqlEmptyAsync(sql);
    }
}