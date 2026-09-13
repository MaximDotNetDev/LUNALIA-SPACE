using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.DependencyInjection;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Infrastructure.Modules.Identity.Authentication;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider) : ICurrentUserService
{
    public RoleType GetUserRole()
    {
        var roleClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)
                     ?? httpContextAccessor.HttpContext?.User?.FindFirst("role");

        if (roleClaim is null || !Enum.TryParse<RoleType>(roleClaim.Value, true, out var role))
        {
            return RoleType.None;
        }

        return role;
    }

    public Guid GetUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
                       ?? httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return Guid.Empty;
        }

        return userId;
    }

    public string? GetClientIp()
    {
        var context = httpContextAccessor.HttpContext;

        if (context is null)
        {
            return null;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
    public async Task<Guid> GetTeacherIdAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Guid.Empty;

        var teacherRepository = serviceProvider.GetRequiredService<ITeacherRepository>();

        var teacher = await teacherRepository.GetDetailsByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

        return teacher?.TeacherId ?? Guid.Empty;
    }
}