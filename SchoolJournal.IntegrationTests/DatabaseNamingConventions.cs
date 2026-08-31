using System.Collections.Frozen;

namespace SchoolJournal.IntegrationTests;

public static partial class DatabaseNamingConventions
{
    public static string GetIgnoreFilter(string alias = "t") =>
            $"{alias}.name NOT LIKE '%History%' " +
            $"AND SCHEMA_NAME({alias}.schema_id) IN ('Identity', 'Core', 'Operations', 'Reference', 'Infrastructure', 'Communications')";

    public const string SqlPascalCaseViolationFilter = "(name LIKE '%[_]%' OR name COLLATE Latin1_General_BIN LIKE '[a-z]%')";

    public static string TablePascalCaseViolationFilter =>
        "(t.name LIKE '%[_]%' OR t.name COLLATE Latin1_General_BIN LIKE '[a-z]%')";

    public static string ColumnPascalCaseViolationFilter =>
        "(c.name LIKE '%[_]%' OR c.name COLLATE Latin1_General_BIN LIKE '[a-z]%')";

    [System.Text.RegularExpressions.GeneratedRegex(@"^[A-Z][a-z0-9]+(?:[A-Z][a-z0-9]+)*$")]
    public static partial System.Text.RegularExpressions.Regex PascalCaseRegex();

    public static readonly FrozenSet<string> ReservedKeywords =
        new[] { "User", "Order", "Group", "Level", "Key", "Table", "Column" }.ToFrozenSet();
}