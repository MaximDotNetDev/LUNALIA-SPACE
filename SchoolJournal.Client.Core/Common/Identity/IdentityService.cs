using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Client.Core.Common.Identity;

public sealed class IdentityService : IIdentityService
{
    public RoleType CurrentRole { get; private set; } = RoleType.None;
    public bool IsAuthenticated => CurrentRole != RoleType.None;

    public void SetUser(RoleType role) => CurrentRole = role;

    public void ClearUser() => CurrentRole = RoleType.None;

    public bool IsInRole(params RoleType[] roles)
    {
        return roles.Contains(CurrentRole);
    }
}