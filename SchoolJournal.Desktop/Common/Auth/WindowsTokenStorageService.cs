using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using SchoolJournal.Client.Core.Common.Auth;

namespace SchoolJournal.Desktop.Common.Auth;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1515:Consider making public types internal")]
public sealed class WindowsTokenStorageService : ITokenStorageService, IDisposable
{
    private readonly string _storageDirectory;
    private readonly string _storageFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public WindowsTokenStorageService()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _storageDirectory = Path.Combine(localAppData, "SchoolJournal");
        _storageFilePath = Path.Combine(_storageDirectory, "tokens.json");
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            var data = new TokenData(accessToken, refreshToken);

            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data);
            byte[] encryptedBytes = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);

            await File.WriteAllBytesAsync(_storageFilePath, encryptedBytes, ct).ConfigureAwait(false);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<(string? AccessToken, string? RefreshToken)> GetTokensAsync(CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_storageFilePath))
            {
                return (null, null);
            }

            try
            {
                byte[] encryptedBytes = await File.ReadAllBytesAsync(_storageFilePath, ct).ConfigureAwait(false);
                byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                var data = JsonSerializer.Deserialize<TokenData>(decryptedBytes);

                return (data?.AccessToken, data?.RefreshToken);
            }
            catch (Exception ex) when (ex is JsonException or CryptographicException)
            {
                return (null, null);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task ClearTokensAsync(CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (File.Exists(_storageFilePath))
            {
                File.Delete(_storageFilePath);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void Dispose()
    {
        _fileLock.Dispose();
    }

    private sealed record TokenData(string AccessToken, string RefreshToken);
}