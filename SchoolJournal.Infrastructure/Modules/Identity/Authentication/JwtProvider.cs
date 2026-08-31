using System.Globalization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Domain.Entities.Identity;

namespace SchoolJournal.Infrastructure.Modules.Identity.Authentication;

public sealed class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Claim[] claims =
            [
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes),
            SigningCredentials = signingCredentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomNumber = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(randomNumber);
    }

    public int GetAccessTokenExpirationSeconds() => _options.ExpirationInMinutes * 60;
}