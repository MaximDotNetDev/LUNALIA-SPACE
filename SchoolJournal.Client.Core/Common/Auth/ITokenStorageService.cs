namespace SchoolJournal.Client.Core.Common.Auth;

public interface ITokenStorageService
{
    public Task SaveTokensAsync(string accessToken, string refreshToken, CancellationToken ct = default);

    public Task<(string? AccessToken, string? RefreshToken)> GetTokensAsync(CancellationToken ct = default);

    public Task ClearTokensAsync(CancellationToken ct = default);
}