using Xunit;
using Dapper;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseIntegrityTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task AllForeignKeysShouldHaveIndexes()
    {
        const string sql = @"
            SELECT fk.name AS ForeignKeyName
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            LEFT JOIN sys.index_columns ic ON fkc.parent_object_id = ic.object_id 
                AND fkc.parent_column_id = ic.column_id
            WHERE ic.object_id IS NULL";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task PrimaryKeysShouldBeIdentity()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE c.is_identity = 0 
          AND ty.name != 'uniqueidentifier' 
          AND t.name != 'AuditLogs'
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotHaveCircularDependencies()
    {
        const string sql = @"
        SELECT OBJECT_NAME(fk1.parent_object_id) + ' <-> ' + OBJECT_NAME(fk1.referenced_object_id)
        FROM sys.foreign_keys fk1
        JOIN sys.foreign_keys fk2 ON fk1.parent_object_id = fk2.referenced_object_id 
            AND fk1.referenced_object_id = fk2.parent_object_id
        WHERE fk1.object_id < fk2.object_id";

        await AssertSqlEmptyAsync(sql);
    }
}