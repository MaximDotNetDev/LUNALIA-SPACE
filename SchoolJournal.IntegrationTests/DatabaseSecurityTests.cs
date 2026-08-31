using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseSecurityTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task AllObjectsShouldBeInAllowedSchemas()
    {
        const string sql = @"
        SELECT name 
        FROM sys.objects 
        WHERE SCHEMA_NAME(schema_id) NOT IN ('Identity', 'Core', 'Operations', 'Reference', 'Infrastructure', 'Communications', 'dbo') 
          AND type IN ('U', 'V', 'P', 'FN', 'IF', 'TF')
          AND is_ms_shipped = 0";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotHaveTriggers()
    {
        const string sql = "SELECT name FROM sys.triggers";
        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotUseCascadeDeleteOnCriticalTables()
    {
        const string sql = @"
        SELECT 
            fk.name AS ForeignKeyName,
            OBJECT_NAME(fk.parent_object_id) AS TableName
        FROM sys.foreign_keys fk
        WHERE fk.delete_referential_action = 1 -- 1 = CASCADE
          AND OBJECT_NAME(fk.parent_object_id) NOT LIKE '%History%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ForeignKeysShouldNotUseCascadeUpdate()
    {
        const string sql = @"
        SELECT name FROM sys.foreign_keys 
        WHERE update_referential_action = 1 -- 1 = CASCADE";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task CriticalForeignKeysShouldBeNotNullable()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.foreign_key_columns fkc
        JOIN sys.tables t ON fkc.parent_object_id = t.object_id
        JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE c.is_nullable = 1 
          AND t.name NOT LIKE '%Optional%'
          AND c.name NOT IN ('SubgroupID', 'PedagogicalTitleID', 'UserID')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AvoidXmlAndLargeBinaryTypes()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('xml', 'varbinary') 
          AND c.max_length = -1 -- -1 означає (MAX)
          AND t.name NOT LIKE '%Attachment%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotUseUserDefinedDataTypes()
    {
        const string sql = "SELECT name FROM sys.types WHERE is_user_defined = 1";
        await AssertSqlEmptyAsync(sql);
    }
}