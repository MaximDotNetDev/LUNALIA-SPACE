using ErrorOr;
using Microsoft.Extensions.Logging;
using Refit;
using SchoolJournal.Client.Core.Common.Auth;
using SchoolJournal.Client.Core.Common.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Contracts.DTOs.Identity.Logout;
using SchoolJournal.Contracts.DTOs.Identity.Refresh;

namespace SchoolJournal.Client.Core.Features.Identity.Common;

public sealed partial class AuthService(
    IIdentityApi identityApi,
    ITokenStorageService tokenStorage,
    ILogger<AuthService> logger,
    IIdentityService identityService) : IAuthService
{

    public async Task<ErrorOr<LoginResponse>> LoginAsync(string login, string password, string? deviceId = null, CancellationToken ct = default)
    {
        var request = new LoginRequest(login, password, deviceId);

        var response = await identityApi.LoginAsync(request, ct).ConfigureAwait(false);

        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            await tokenStorage.SaveTokensAsync(
                response.Content.AccessToken,
                response.Content.RefreshToken,
                ct).ConfigureAwait(false);

            identityService.SetUser(response.Content.Role);

            return response.Content;
        }

        if (response.Error is ApiException apiEx)
        {
            return await ParseErrorResponseAsync<LoginResponse>(apiEx).ConfigureAwait(false);
        }
        
        return Error.Unexpected(description: "Сталася невідома помилка під час зв'язку із сервером.");
    }

    private static async Task<ErrorOr<T>> ParseErrorResponseAsync<T>(ApiException error)
    {
        try
        {
            var problem = await error.GetContentAsAsync<ProblemDetailsDto>().ConfigureAwait(false);
            if (problem is not null)
            {
                if (problem.Errors is not null && problem.Errors.Count > 0)
                {
                    var firstError = problem.Errors.First().Value.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(firstError))
                    {
                        return Error.Validation(description: firstError);
                    }
                }

                if (!string.IsNullOrWhiteSpace(problem.Detail))
                {
                    return Error.Validation(description: problem.Detail);
                }

                if (!string.IsNullOrWhiteSpace(problem.Title))
                {
                    return Error.Validation(description: problem.Title);
                }
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            return Error.Unexpected(
                code: "Api.ParseError",
                description: $"Не вдалося розпізнати формат помилки від сервера (невірний JSON): {ex.Message}");
        }

        return Error.Unexpected(description: "Сталася невідома помилка під час зв'язку із сервером.");
    }

    public async Task<ErrorOr<Success>> RefreshTokenAsync(CancellationToken ct = default)
    {
        var (_, refreshToken) = await tokenStorage.GetTokensAsync(ct).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await LogoutAsync(ct).ConfigureAwait(false);
            return Error.Unauthorized(code: "Auth.NoToken", description: "Токен оновлення відсутній на пристрої.");
        }

        var request = new RefreshTokenRequest(refreshToken, null);

        var response = await identityApi.RefreshAsync(request, ct).ConfigureAwait(false);

        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            await tokenStorage.SaveTokensAsync(
                response.Content.AccessToken,
                response.Content.RefreshToken,
                ct).ConfigureAwait(false);

            identityService.SetUser(response.Content.Role);

            return Result.Success;
        }

        await LogoutAsync(ct).ConfigureAwait(false);

        if (response.Error is ApiException apiEx)
        {
            return await ParseErrorResponseAsync<Success>(apiEx).ConfigureAwait(false);
        }

        return Error.Unexpected(description: "Сталася невідома помилка під час оновлення токена.");
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try
        {
            var (_, refreshToken) = await tokenStorage.GetTokensAsync(ct).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var request = new LogoutRequest(refreshToken);
                await identityApi.LogoutAsync(request, ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
        {
            LogLogoutWarning(logger, ex);
        }
        finally
        {
            try
            {
                await tokenStorage.ClearTokensAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                LogTokenClearError(logger, ex);
            }
            finally
            {
                identityService.ClearUser();
            }
        }
    }

    [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Warning,
            Message = "Не вдалося виконати серверний Logout. Відбудеться примусове локальне очищення.")]
    private static partial void LogLogoutWarning(ILogger logger, Exception ex);

    [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Error,
            Message = "Критична помилка: Не вдалося очистити локальне сховище токенів через проблеми з доступом до файлу.")]
    private static partial void LogTokenClearError(ILogger logger, Exception ex);
}

public sealed record ProblemDetailsDto(
        string? Title,
        int? Status,
        string? Detail,
        Dictionary<string, string[]>? Errors);
