using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Testcontainers.MsSql;
using Xunit;

namespace SchoolJournal.IntegrationTests;

public sealed partial class MsSqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;

    private string MasterConnectionString =>
        $"{_msSqlContainer.GetConnectionString()};Connect Timeout=120;TrustServerCertificate=True;Encrypt=False;Pooling=False;Application Name=SchoolJournalTests;";

    public string ConnectionString =>
        $"{_msSqlContainer.GetConnectionString()};Connect Timeout=120;Database=DB_SchoolJournal;TrustServerCertificate=True;Encrypt=False;Pooling=False;";

    public MsSqlFixture()
    {
        _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithCleanUp(true)
            .Build();
    }
    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();

        var baseDir = AppContext.BaseDirectory;
        var sqlFilePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "DB_SchoolJournal.sql"));

        if (!File.Exists(sqlFilePath))
            throw new FileNotFoundException("Critical Error: Database schema script not found!", sqlFilePath);

        var script = await File.ReadAllTextAsync(sqlFilePath);
        if (string.IsNullOrWhiteSpace(script))
            throw new InvalidOperationException("Critical Error: Database schema script is empty!");

        await ExecuteScriptWithRetryAsync(script);
    }

    private async Task ExecuteScriptWithRetryAsync(string script)
    {
        var batches = GoBatchRegex().Split(script);
        const int maxRetries = 3;

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                await ExecuteBatchesAsync(batches);
                break;
            }
            catch (SqlException ex) when (ex.Message.Contains("transport", StringComparison.OrdinalIgnoreCase) ||
                                                      ex.Message.Contains("forcibly closed", StringComparison.OrdinalIgnoreCase) ||
                                                      ex.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
                                                      ex.Message.Contains("Login failed", StringComparison.OrdinalIgnoreCase))
            {
                if (i == maxRetries) throw;
                await Task.Delay(5000); 
            }
        }
    }

    private async Task ExecuteBatchesAsync(IEnumerable<string> batches)
    {
        using var connection = new SqlConnection(MasterConnectionString);
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            if (!string.IsNullOrWhiteSpace(batch))
            {
                await connection.ExecuteAsync(batch, commandTimeout: 300);
            }
        }
    }

    public async Task DisposeAsync()
    {
        if (_msSqlContainer != null)
        {
            await _msSqlContainer.StopAsync();
            await _msSqlContainer.DisposeAsync();
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex GoBatchRegex();
}