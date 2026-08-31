using SchoolJournal.Domain.Entities.Identity;

namespace SchoolJournal.Application.Features.Identity.Common.Interfaces;

public interface IJwtProvider
{
    public string GenerateAccessToken(User user);
    public string GenerateRefreshToken();
    public int GetAccessTokenExpirationSeconds();
}