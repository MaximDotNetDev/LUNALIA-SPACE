using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Identity.Common.Errors;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;

namespace SchoolJournal.Application.Features.Identity.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    ICurrentUserService currentUserService)
    : IRequestHandler<LoginCommand, ErrorOr<TokenResponse>>
{
    public async Task<ErrorOr<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await userRepository.GetByLoginAsync(request.Login, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return IdentityErrors.InvalidCredentials;
        }

        if (!user.IsActive || user.IsDeleted)
        {
            return IdentityErrors.UserInactive;
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTimeOffset.UtcNow)
        {
            return IdentityErrors.UserLockedOut;
        }

        bool isPasswordValid = passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!isPasswordValid)
        {
            var updatedUser = user with { FailedLoginAttempts = user.FailedLoginAttempts + 1 };

            if (updatedUser.FailedLoginAttempts >= 5)
            {
                updatedUser = updatedUser with { LockoutEndUtc = DateTimeOffset.UtcNow.AddMinutes(15) };
            }

            await userRepository.UpdateAsync(updatedUser, cancellationToken).ConfigureAwait(false);

            return IdentityErrors.InvalidCredentials;
        }

        user = user with
        {
            FailedLoginAttempts = 0,
            LastLoginUtc = DateTimeOffset.UtcNow
        };

        await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

        string accessToken = jwtProvider.GenerateAccessToken(user);
        string refreshTokenPlain = jwtProvider.GenerateRefreshToken();
        int expiresIn = jwtProvider.GetAccessTokenExpirationSeconds();

        var refreshToken = new RefreshToken
        {
            TokenId = Guid.CreateVersion7(),
            UserId = user.UserId,
            TokenHash = System.Convert.ToBase64String(System.Security.Cryptography.SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(refreshTokenPlain))),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByIp = currentUserService.GetClientIp(),
            DeviceIdentifier = request.DeviceIdentifier,
            IsDeleted = false,
            Revoked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = []
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        return new TokenResponse(accessToken, refreshTokenPlain, expiresIn);
    }
}