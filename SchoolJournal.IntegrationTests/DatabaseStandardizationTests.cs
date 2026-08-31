using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;
using System.Collections.Frozen;

namespace SchoolJournal.IntegrationTests;

public sealed class DatabaseStandardizationTests(MsSqlFixture fixture) : DatabaseTestBase(fixture)
{
    [Fact]
    public async Task ColumnsShouldHaveConsistentCollation()
    {
        string sql = $@"
        SELECT t.name AS TableName, c.name AS ColumnName
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.collation_name IS NOT NULL 
          AND c.collation_name != DATABASEPROPERTYEX(DB_NAME(), 'Collation')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task DetectPotentialMissingForeignKeys()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%Id' OR c.name = 'Id') 
        AND c.name NOT LIKE 'ledger_%'
          AND NOT (t.name = 'CoinTransactions' AND c.name = 'ReferenceId')
            AND {DatabaseNamingConventions.GetIgnoreFilter()}
          AND NOT EXISTS (
            SELECT 1 FROM sys.foreign_key_columns fkc 
              WHERE fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
          )
          AND NOT EXISTS (
              SELECT 1 FROM sys.index_columns ic
              JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
              WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id AND i.is_primary_key = 1
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task NaturalKeysShouldHaveUniqueConstraints()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%Email%' OR c.name LIKE '%Phone%' OR c.name LIKE '%Code%')
          AND NOT EXISTS (
              SELECT 1 FROM sys.index_columns ic
              JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
              WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id AND i.is_unique = 1
          )";

        await AssertSqlEmptyAsync(sql);
    }

    private static readonly FrozenSet<string> CriticalTables =
                new[] { "Students", "Grades", "Teachers" }.ToFrozenSet();

    [Fact]
    public async Task CriticalTablesShouldHaveRowVersion()
    {
        string sql = $@"
            SELECT t.name 
            FROM sys.tables t 
            WHERE t.name IN @TableNames
              AND NOT EXISTS (
                  SELECT 1 FROM sys.columns c 
                  JOIN sys.types ty ON c.user_type_id = ty.user_type_id
                  WHERE c.object_id = t.object_id AND ty.name = 'timestamp'
              )";

        await AssertSqlEmptyAsync(sql, new { TableNames = CriticalTables });
    }

    [Fact]
    public async Task AllTablesShouldHaveClusteredIndex()
    {
        string sql = $@"
            SELECT t.name 
            FROM sys.tables t 
            WHERE t.object_id NOT IN (
                SELECT object_id FROM sys.indexes WHERE type = 1
            ) AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ForeignKeyTypesShouldMatchPrimaryKeys()
    {
        string sql = $@"
            SELECT 
                OBJECT_NAME(fkc.parent_object_id) + '.' + pc.name AS ForeignKeyColumn
            FROM sys.foreign_key_columns fkc
            JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
            JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
            WHERE pc.system_type_id != rc.system_type_id OR pc.max_length != rc.max_length";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task DefaultConstraintsShouldBeNamed()
    {
        string sql = $@"
        SELECT name 
        FROM sys.default_constraints 
        WHERE is_system_named = 1";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task IndexesShouldFollowNamingConvention()
    {
        string sql = $@"
        SELECT i.name 
        FROM sys.indexes i
        JOIN sys.tables t ON i.object_id = t.object_id
        WHERE i.is_primary_key = 0 
          AND i.is_unique_constraint = 0 
          AND i.type > 0 -- не куча
            AND i.name NOT LIKE 'IX[_]%'
            AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ViewsShouldNotUseSelectStar()
    {
        string sql = $@"
        SELECT OBJECT_NAME(m.object_id) 
        FROM sys.sql_modules m
        JOIN sys.views v ON m.object_id = v.object_id
        WHERE m.definition LIKE '%SELECT *%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotUseScalarFunctions()
    {
        const string sql = "SELECT name FROM sys.objects WHERE type = 'FN'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ColumnPrefixesShouldMatchDataTypes()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE (c.name LIKE 'Is%' AND ty.name != 'bit')
           OR (c.name LIKE '%At' AND ty.name NOT IN ('datetime2', 'datetimeoffset'))";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task TablesShouldNotHaveTooManyColumns()
    {
        string sql = $@"
        SELECT name 
        FROM sys.tables 
        WHERE object_id IN (
            SELECT object_id FROM sys.columns GROUP BY object_id HAVING COUNT(*) > 30
        )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotHaveRedundantIndexes()
    {
        string sql = $@"
        SELECT i1.name AS RedundantIndex, i2.name AS ParentIndex
        FROM sys.indexes i1
        JOIN sys.indexes i2 ON i1.object_id = i2.object_id AND i1.index_id <> i2.index_id
        JOIN sys.tables t ON i1.object_id = t.object_id
        WHERE i1.type > 0 AND i2.type > 0
          AND {DatabaseNamingConventions.GetIgnoreFilter()}
          AND i1.has_filter = 0 AND i2.has_filter = 0
          AND i1.name NOT IN ('UQ_QuizSubmissions_Student_Assignment', 'IX_QuizSubmissions_StudentId', 'UQ_Wallets_Student_Subject', 'IX_Wallets_StudentId')
          AND EXISTS (
              SELECT 1 FROM sys.index_columns ic1
              JOIN sys.index_columns ic2 ON ic1.object_id = ic2.object_id 
                AND ic1.index_id = i1.index_id AND ic2.index_id = i2.index_id
                AND ic1.key_ordinal = ic2.key_ordinal AND ic1.column_id = ic2.column_id
              WHERE ic1.key_ordinal = 1 AND ic1.object_id = i1.object_id
          )
          AND i1.is_primary_key = 0 AND i2.is_primary_key = 0";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task TablesShouldNotHaveTooManyIndexes()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t
        WHERE (SELECT COUNT(*) FROM sys.indexes i WHERE i.object_id = t.object_id AND i.type > 0) > 10
            AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task UniqueIndexesOnSoftDeleteTablesShouldBeFiltered()
    {
        string sql = $@"
        SELECT i.name 
        FROM sys.indexes i
        JOIN sys.tables t ON i.object_id = t.object_id
        JOIN sys.columns c ON t.object_id = c.object_id AND c.name = 'IsDeleted'
        WHERE i.is_unique = 1 
          AND i.has_filter = 0 -- немає фільтра
          AND i.is_primary_key = 0
          AND i.name NOT IN ('UQ_QuizSubmissions_Student_Assignment', 'UQ_Wallets_Student_Subject')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task IdenticalColumnsAcrossTablesShouldHaveSameLength()
    {
        string sql = $@"
        SELECT c1.name
        FROM sys.columns c1
        JOIN sys.tables t1 ON c1.object_id = t1.object_id
        JOIN sys.columns c2 ON c1.name = c2.name
        JOIN sys.tables t2 ON c2.object_id = t2.object_id
        WHERE c1.name IN ('Email', 'PhoneNumber', 'TaxId', 'Code')
          AND (c1.max_length != c2.max_length OR c1.system_type_id != c2.system_type_id)
          AND t1.object_id < t2.object_id";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ShouldNotHaveIsolatedTables()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t
        WHERE {DatabaseNamingConventions.GetIgnoreFilter()}
          AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk WHERE fk.parent_object_id = t.object_id)
          AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk WHERE fk.referenced_object_id = t.object_id)";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AvoidLargeFixedLengthStrings()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('char', 'nchar') 
          AND c.max_length > 20";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AvoidFloatingPointTypesForPrecision()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('float', 'real')
          AND t.name NOT LIKE '%Log%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task BitColumnsShouldHaveDefaultValues()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name = 'bit' 
          AND c.is_nullable = 0 
          AND c.default_object_id = 0
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllConstraintsShouldFollowNamingConvention()
    {
        string sql = $@"
        SELECT name FROM sys.objects 
        WHERE (type = 'PK' AND name NOT LIKE 'PK[_]%')
           OR (type = 'F' AND name NOT LIKE 'FK[_]%')
           OR (type = 'UQ' AND name NOT LIKE 'UQ[_]%')
           OR (type = 'C' AND name NOT LIKE 'CK[_]%')";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task PrimaryKeyShouldBeTheFirstColumn()
    {
        string sql = $@"
        SELECT t.name
        FROM sys.tables t
        JOIN sys.index_columns ic ON t.object_id = ic.object_id
        JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
         WHERE i.is_primary_key = 1 
          AND c.column_id != 1
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task IdentitySeedAndIncrementShouldBeOne()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.identity_columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.seed_value != 1 OR c.increment_value != 1)
          AND t.is_ms_shipped = 0";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ForeignKeyNamesShouldFollowStandardPattern()
    {
        string sql = $@"
        SELECT fk.name 
        FROM sys.foreign_keys fk
        WHERE fk.name NOT LIKE 'FK_' + OBJECT_NAME(fk.parent_object_id) + '_' + OBJECT_NAME(fk.referenced_object_id) + '%'
          AND fk.name != 'FK_QuizSubmissions_Assignments'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllPrimaryKeysShouldHaveSameType()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name NOT IN ('uniqueidentifier', 'bigint')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task StoredProceduresShouldNotUseSelectStar()
    {
        string sql = $@"
        SELECT OBJECT_NAME(m.object_id) 
        FROM sys.sql_modules m
        JOIN sys.procedures p ON m.object_id = p.object_id
        WHERE m.definition LIKE '%SELECT *%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task IdentityShouldOnlyBeUsedOnPrimaryKeys()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.is_identity = 1
          AND t.is_ms_shipped = 0
          AND NOT EXISTS (
              SELECT 1 FROM sys.index_columns ic
              JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
              WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id AND i.is_primary_key = 1
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task HistoricalDateColumnsShouldNotAllowFutureDates()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%BirthDate%' OR c.name LIKE '%EnrollmentDate%')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}
          AND NOT EXISTS (
            SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.parent_column_id = c.column_id
                AND (cc.definition LIKE '%<=%GET%DATE%' OR cc.definition LIKE '%<%GET%DATE%')
          )";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllBitColumnsShouldBeNonNullable()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name = 'bit' AND c.is_nullable = 1";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task TextColumnsShouldNotAllowLeadingTrailingSpaces()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name IN ('nvarchar', 'varchar')
          AND c.is_nullable = 0
          AND NOT EXISTS (
              SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.parent_column_id = c.column_id
            AND cc.definition LIKE '%LTRIM%' AND cc.definition LIKE '%RTRIM%'
          )
          AND NOT (t.name = 'CoinTransactions' AND c.name = 'TransactionType')
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task DefaultsShouldOnlyUseUtcTime()
    {
        string sql = $@"
        SELECT name 
        FROM sys.default_constraints 
            WHERE (definition LIKE '%GETDATE%' OR definition LIKE '%SYSDATETIME%') 
            AND definition NOT LIKE '%UTC%'";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task PrimaryKeysShouldNotBeGuidsIfClustered()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name = 'uniqueidentifier' 
          AND i.type = 1 
          AND c.default_object_id = 0 -- Сваримось тільки якщо немає NEWSEQUENTIALID()";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ObjectsInModulesShouldBeSchemaQualified()
    {
        string sql = $@"
        SELECT OBJECT_NAME(m.object_id)
        FROM sys.sql_modules m
        WHERE m.definition NOT LIKE '%].\[%' ESCAPE '\' 
          AND OBJECTPROPERTY(m.object_id, 'IsMsShipped') = 0";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task PrimaryKeysShouldBeClustered()
    {
        string sql = $@"
        SELECT t.name 
        FROM sys.tables t
        JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
        WHERE i.type != 1 AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ComputedColumnsShouldBePersisted()
    {
        const string sql = "SELECT name FROM sys.computed_columns WHERE is_computed = 1 AND is_persisted = 0";
        
        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task IndexedColumnsShouldHaveReasonableLength()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        JOIN sys.index_columns ic ON c.object_id = ic.object_id AND c.column_id = ic.column_id
        JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        WHERE ty.name = 'nvarchar' AND c.max_length > 900";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task EnumLikeColumnsShouldHaveCheckConstraints()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE (c.name LIKE '%Status' OR c.name LIKE '%Type' OR c.name LIKE '%Gender')
          AND NOT EXISTS (
              SELECT 1 FROM sys.check_constraints cc 
              WHERE cc.parent_object_id = t.object_id AND cc.parent_column_id = c.column_id
          )
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task AllCheckConstraintsShouldBeTrusted()
    {
        string sql = $@"
        SELECT name 
        FROM sys.check_constraints 
        WHERE is_not_trusted = 1 OR is_disabled = 1";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ColumnsShouldNotIncludeTableName()
    {
        string sql = $@"
        SELECT t.name + '.' + c.name
        FROM sys.columns c
        JOIN sys.tables t ON c.object_id = t.object_id
        WHERE c.name LIKE t.name + '%' 
          AND c.name != t.name + 'Id' -- Дозволяємо тільки PK/FK
          AND {DatabaseNamingConventions.GetIgnoreFilter()}";

        await AssertSqlEmptyAsync(sql);
    }

    [Fact]
    public async Task ModulesShouldHaveStandardSettings()
    {
        string sql = $@"
        SELECT name 
        FROM sys.sql_modules m
        JOIN sys.objects o ON m.object_id = o.object_id
        WHERE m.uses_ansi_nulls = 0 OR m.uses_quoted_identifier = 0";

        await AssertSqlEmptyAsync(sql);
    }
}