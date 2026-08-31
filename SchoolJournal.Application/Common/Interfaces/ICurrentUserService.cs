using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Common.Interfaces;

public interface ICurrentUserService
{
    public Guid GetUserId();
    public RoleType GetUserRole();
    public string? GetClientIp();
}