using Xunit;
using Dapper;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseSchemaComplexTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task GradeTableShouldHaveRangeConstraint()
    {
        const string sql = @"
            SELECT cc.name 
            FROM sys.check_constraints cc
            JOIN sys.tables t ON cc.parent_object_id = t.object_id
            WHERE (t.name LIKE '%Grade%' OR t.name LIKE '%Mark%') 
              AND (cc.definition LIKE '%Value%' OR cc.definition LIKE '%Score%')";

        var constraints = await DbConnection.QueryAsync<string>(sql);
        Assert.NotEmpty(constraints);
    }

    [Fact]
    public async Task CriticalTablesShouldBeSystemVersioned()
    {
        var temporalRequired = new[] { "Grades", "Students" };
        const string sql = "SELECT name FROM sys.tables WHERE temporal_type = 2";

        var versionedTables = await DbConnection.QueryAsync<string>(sql);

        foreach (var table in temporalRequired)
        {
            Assert.Contains(table, versionedTables);
        }
    }

    [Fact]
    public async Task JunctionTablesShouldHaveCompositePrimaryKeys()
    {
        const string sql = @"
        SELECT t.name 
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        WHERE t.name LIKE '%To%' AND t.name != 'RefreshTokens'
          AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id = t.object_id AND ic.index_id = i.index_id) < 2";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task JunctionTablesShouldHaveReverseIndex()
    {
        const string sql = @"
        SELECT t.name 
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        WHERE t.name LIKE '%To%' AND t.name != 'RefreshTokens'
          AND ic.key_ordinal = 1
          AND NOT EXISTS (
              SELECT 1 FROM sys.indexes i2
              JOIN sys.index_columns ic2 ON i2.object_id = ic2.object_id AND i2.index_id = ic2.index_id
              WHERE i2.object_id = t.object_id 
                AND i2.is_primary_key = 0
                AND ic2.column_id = (SELECT column_id FROM sys.index_columns WHERE object_id = t.object_id AND index_id = i.index_id AND key_ordinal = 2)
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task EmailColumnsShouldHaveAtSymbolConstraint()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.name LIKE '%Email%'
          AND NOT EXISTS (
              SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.definition LIKE '%@%'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task JsonColumnsShouldHaveIsJsonConstraint()
    {
        const string sql = @"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%Json' OR c.name LIKE '%Metadata')
          AND NOT EXISTS (
              SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.parent_column_id = c.column_id
                AND cc.definition LIKE '%ISJSON%'
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task MandatoryTextColumnsShouldNotAllowEmptyStrings()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE c.is_nullable = 0 
        AND ty.name IN ('nvarchar', 'varchar', 'char', 'nchar')
          AND (c.name LIKE '%Name%' OR c.name LIKE '%Title%' OR c.name LIKE '%Subject%')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}
          AND NOT EXISTS (
            SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.parent_column_id = c.column_id
                AND cc.definition LIKE '%LEN%>%0%'
          )";

        await AssertSqlEmptyAsync(sql);
    }
}