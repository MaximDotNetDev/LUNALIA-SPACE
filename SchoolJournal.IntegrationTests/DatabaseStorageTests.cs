using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseStorageTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task AllDateTimeColumnsShouldUseDateTimeOffset()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('datetime', 'datetime2', 'date', 'time') 
          AND c.name NOT IN ('SysStartTime', 'SysEndTime')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotUseDeprecatedDataTypes()
    {
        const string sql = @"
            SELECT t.name AS TableName, c.name AS ColumnName
            FROM sys.columns c
            JOIN sys.tables t ON c.object_id = t.object_id
            JOIN sys.types ty ON c.user_type_id = ty.user_type_id
            WHERE ty.name IN ('text', 'ntext', 'image')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllTextColumnsShouldBeUnicode()
    {
        string sql = $@"
        SELECT t.name AS TableName, c.name AS ColumnName
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('varchar', 'char') 
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotUseMoneyType()
    {
        const string sql = @"
        SELECT t.name AS TableName, c.name AS ColumnName
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('money', 'smallmoney')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AvoidUnnecessaryMaxColumns()
    {
        const string sql = @"
        SELECT t.name AS TableName, c.name AS ColumnName
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('nvarchar', 'varchar') 
          AND c.max_length = -1
          AND c.name NOT LIKE '%Description%' 
          AND c.name NOT LIKE '%Content%'
          AND c.name NOT LIKE '%Comment%'
          AND c.name NOT LIKE '%Value%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task DecimalColumnsShouldHaveConsistentScale()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name = 'decimal' AND c.scale != 2 
          AND (c.name LIKE '%Score%' OR c.name LIKE '%Grade%' OR c.name LIKE '%Average%')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task SensitiveDataShouldBeMasked()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%Phone%' OR c.name LIKE '%Address%' OR c.name LIKE '%TaxId%')
          AND c.is_masked = 0";

        await AssertSqlEmptyAsync(sql);
    }
}