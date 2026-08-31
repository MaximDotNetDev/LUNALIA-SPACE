using Refit;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Contracts.DTOs.Identity.Refresh;
using SchoolJournal.Contracts.DTOs.Identity.Logout;
using SchoolJournal.Contracts.DTOs.Identity.Register;

namespace SchoolJournal.Client.Core.Features.Identity.Common;

public interface IIdentityApi
{
    [Post("/api/identity/login")]
    public Task<IApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);

    [Post("/api/identity/refresh")]
    public Task<IApiResponse<LoginResponse>> RefreshAsync([Body] RefreshTokenRequest request, CancellationToken cancellationToken = default);

    [Post("/api/identity/logout")]
    public Task LogoutAsync([Body] LogoutRequest request, CancellationToken cancellationToken = default);

    [Put("/api/identity/users/{userId}")]
    public Task<IApiResponse> UpdateAccountAsync(Guid userId, [Body] SchoolJournal.Contracts.DTOs.Identity.UpdateAccount.UpdateAccountRequest request, CancellationToken cancellationToken = default);

    [Post("/api/identity/register")]
    public Task<IApiResponse<RegisterResponse>> RegisterAsync([Body] RegisterRequest request, CancellationToken cancellationToken = default);
}