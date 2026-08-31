using ErrorOr;
using MediatR;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Domain.Entities.Identity.IRepositories;


namespace SchoolJournal.Application.Features.Identity.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string tokenHash = System.Convert.ToBase64String(System.Security.Cryptography.SHA512.HashData(System.Text.Encoding.UTF8.GetBytes(request.RefreshToken)));

        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken).ConfigureAwait(false);

        if (existingToken is null || existingToken.Revoked)
        {
            return Result.Success;
        }

        var revokedToken = existingToken.Revoke();

        await refreshTokenRepository.UpdateAsync(revokedToken, cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}