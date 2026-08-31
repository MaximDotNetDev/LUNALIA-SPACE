using Microsoft.JSInterop;
using SchoolJournal.Client.Core.Common.Auth;

namespace SchoolJournal.Blazor.Client.Services;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI container")]
internal sealed class BrowserTokenStorageService(IJSRuntime jsRuntime) : ITokenStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public async Task SaveTokensAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", ct, AccessTokenKey, accessToken).ConfigureAwait(false);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", ct, RefreshTokenKey, refreshToken).ConfigureAwait(false);
    }

    public async Task<(string? AccessToken, string? RefreshToken)> GetTokensAsync(CancellationToken ct = default)
    {
        var accessToken = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ct, AccessTokenKey).ConfigureAwait(false);
        var refreshToken = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ct, RefreshTokenKey).ConfigureAwait(false);

        return (accessToken, refreshToken);
    }

    public async Task ClearTokensAsync(CancellationToken ct = default)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", ct, AccessTokenKey).ConfigureAwait(false);
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", ct, RefreshTokenKey).ConfigureAwait(false);
    }
}