using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Contracts.DTOs.Identity.Login;
using SchoolJournal.Application.Features.Identity.Common.Errors;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Domain.Entities.Identity.IRepositories;


namespace SchoolJournal.Application.Features.Identity.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtProvider jwtProvider,
    SchoolJournal.Application.Common.Interfaces.ICurrentUserService currentUserService)
    : IRequestHandler<RefreshTokenCommand, ErrorOr<TokenResponse>>
{
    public async Task<ErrorOr<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string incomingTokenHash = System.Convert.ToBase64String(System.Security.Cryptography.SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(request.RefreshToken)));

        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(incomingTokenHash, cancellationToken).ConfigureAwait(false);

        if (existingToken is null || existingToken.Revoked || existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken).ConfigureAwait(false);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return IdentityErrors.UserInactive;
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTimeOffset.UtcNow)
        {
            return IdentityErrors.UserLockedOut;
        }

        string newAccessToken = jwtProvider.GenerateAccessToken(user);
        string newRefreshTokenPlain = jwtProvider.GenerateRefreshToken();
        string newRefreshTokenHash = System.Convert.ToBase64String(System.Security.Cryptography.SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(newRefreshTokenPlain)));
        int expiresIn = jwtProvider.GetAccessTokenExpirationSeconds();

        var revokedToken = existingToken.Revoke() with
        {
            ReplacedByTokenHash = newRefreshTokenHash
        };

        await refreshTokenRepository.UpdateAsync(revokedToken, cancellationToken).ConfigureAwait(false);

        var newToken = new RefreshToken
        {
            TokenId = Guid.CreateVersion7(),
            UserId = user.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedByIp = currentUserService.GetClientIp(),
            DeviceIdentifier = request.DeviceIdentifier,
            IsDeleted = false,
            Revoked = false,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = []
        };

        await refreshTokenRepository.AddAsync(newToken, cancellationToken).ConfigureAwait(false);

        return new TokenResponse(newAccessToken, newRefreshTokenPlain, expiresIn);
    }
}