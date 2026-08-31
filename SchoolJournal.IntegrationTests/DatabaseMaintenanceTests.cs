using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseMaintenanceTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task TablesShouldNotHaveHighIndexFragmentation()
    {
        const string sql = @"
        SELECT OBJECT_NAME(object_id) AS TableName
        FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'DETAILED')
        WHERE avg_fragmentation_in_percent > 30 AND index_id > 0 AND page_count > 50";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllTablesShouldUseDataCompression()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.partitions p
        JOIN sys.tables t ON p.object_id = t.object_id
        WHERE p.data_compression = 0 
            AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task TablesShouldHaveDescriptions()
    {
        string sql = $@"{Environment.NewLine}        SELECT t.name 
        FROM sys.tables t
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE {DatabaseNamingConventions.GetIgnoreFilter()}
          AND s.name NOT IN ('Identity', 'Infrastructure', 'Reference')
          AND NOT EXISTS (
            SELECT 1 FROM sys.extended_properties ep 
              WHERE ep.major_id = t.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ColumnsShouldHaveDescriptions()
    {
        string sql = $@"{Environment.NewLine}        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE {DatabaseNamingConventions.GetIgnoreFilter()}
          AND s.name IN ('Core', 'Operations')
          AND c.name NOT IN ('CreatedAt', 'UpdatedAt', 'IsDeleted', 'RowVersion', 'SysStartTime', 'SysEndTime')
          AND NOT EXISTS (
              SELECT 1 FROM sys.extended_properties ep 
              WHERE ep.major_id = t.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotHaveDisabledIndexes()
    {
        const string sql = @"
        SELECT i.name 
        FROM sys.indexes i
        JOIN sys.tables t ON i.object_id = t.object_id
        WHERE i.is_disabled = 1";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task DatabaseOptionsShouldBeOptimized()
    {
        const string sql = @"
        SELECT name FROM sys.databases 
        WHERE name = DB_NAME() 
          AND (is_auto_shrink_on = 1 OR is_auto_close_on = 1)";

        await AssertSqlEmptyAsync(sql);
    }
}