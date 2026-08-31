using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Client.Core.Features.Identity.Common;
using SchoolJournal.Client.Core.Common.Auth;

namespace SchoolJournal.Client.Core.Common.Auth;

public sealed class AuthHeaderHandler(
    ITokenStorageService tokenStorage,
    IServiceProvider serviceProvider) : DelegatingHandler
{
    private static readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Content is not null)
        {
            // Передаємо CancellationToken для коректного переривання операції буферизації
            await request.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.ExpectContinue = true;
        }

        var (accessToken, _) = await tokenStorage.GetTokensAsync(cancellationToken).ConfigureAwait(false);
        var isAuthEndpoint = request.RequestUri?.AbsolutePath.Contains("login", StringComparison.OrdinalIgnoreCase) == true;

        if (!string.IsNullOrWhiteSpace(accessToken) && !isAuthEndpoint)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Unauthorized ||
            request.RequestUri?.AbsolutePath.Contains("refresh", StringComparison.OrdinalIgnoreCase) == true ||
            request.RequestUri?.AbsolutePath.Contains("logout", StringComparison.OrdinalIgnoreCase) == true)
        {
            return response;
        }

        await _refreshSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var (currentToken, _) = await tokenStorage.GetTokensAsync(cancellationToken).ConfigureAwait(false);

            if (!string.Equals(currentToken, accessToken, StringComparison.Ordinal))
            {
                response.Dispose();
                using var clonedRequestConcurrent = await CloneRequestAsync(request).ConfigureAwait(false);
                clonedRequestConcurrent.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
                return await base.SendAsync(clonedRequestConcurrent, cancellationToken).ConfigureAwait(false);
            }

            using var scope = serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            var refreshResult = await authService.RefreshTokenAsync(cancellationToken).ConfigureAwait(false);

            if (refreshResult.IsError)
            {
                await authService.LogoutAsync(cancellationToken).ConfigureAwait(false);
                return response;
            }

            var (newToken, _) = await tokenStorage.GetTokensAsync(cancellationToken).ConfigureAwait(false);

            response.Dispose();

            using var clonedRequest = await CloneRequestAsync(request).ConfigureAwait(false);
            clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

            return await base.SendAsync(clonedRequest, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            clone.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        return clone;
    }
}