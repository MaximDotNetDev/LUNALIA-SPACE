using ErrorOr;
using SchoolJournal.Contracts.DTOs.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Client.Core.Features.Identity.Common;

public interface IAuthService
{
    public Task<ErrorOr<LoginResponse>> LoginAsync(string login, string password, string? deviceId = null, CancellationToken ct = default);

    public Task<ErrorOr<Success>> RefreshTokenAsync(CancellationToken ct = default);

    public Task LogoutAsync(CancellationToken ct = default);
}