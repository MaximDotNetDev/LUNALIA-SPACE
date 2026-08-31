using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseAuditTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task TablesShouldHaveAuditColumns()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t 
            WHERE {DatabaseNamingConventions.GetIgnoreFilter()}
            AND NOT EXISTS (
              SELECT 1 FROM sys.columns c 
              WHERE c.object_id = t.object_id AND c.name = 'CreatedAt'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AuditColumnsShouldBeNonNullable()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.name IN ('CreatedAt', 'IsDeleted') 
          AND c.is_nullable = 1";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AuditColumnsShouldHaveDefaultValues()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.name = 'CreatedAt' 
          AND c.default_object_id = 0
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AuditColumnsShouldBeSymmetric()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t 
        WHERE EXISTS (SELECT 1 FROM sys.columns WHERE object_id = t.object_id AND name = 'CreatedAt')
          AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = t.object_id AND name = 'UpdatedAt')
            AND t.name != 'AuditLogs'
          AND t.name != 'CoinTransactions'
            AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task TablesShouldSupportSoftDelete()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t 
            WHERE {DatabaseNamingConventions.GetIgnoreFilter()}
            AND t.name != 'AuditLogs'
            AND t.name != 'CoinTransactions'
            AND NOT EXISTS (
            SELECT 1 FROM sys.columns c 
              JOIN sys.types ty ON c.user_type_id = ty.user_type_id
              WHERE c.object_id = t.object_id 
                AND c.name = 'IsDeleted' 
                AND ty.name = 'bit'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task SoftDeleteColumnsShouldBeIndexed()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.name = 'IsDeleted'
          AND t.name NOT IN ('QuizQuestions', 'Wallets')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}
          AND NOT EXISTS (
            SELECT 1 FROM sys.index_columns ic
              JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
              WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task SoftDeleteTablesShouldHaveFilteredUniqueIndex()
    {
        string sql = $@"{Environment.NewLine}        SELECT t.name 
        FROM sys.tables t
        WHERE EXISTS (SELECT 1 FROM sys.columns WHERE object_id = t.object_id AND name = 'IsDeleted')
          AND t.name NOT IN ('AuditLogs', 'OutboxMessages', 'Announcements')
          AND NOT EXISTS (
              SELECT 1 FROM sys.indexes i 
              WHERE i.object_id = t.object_id AND i.is_unique = 1 AND i.has_filter = 1
          )
            AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }
}