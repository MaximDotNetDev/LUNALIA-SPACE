using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Client.Core.Common.Identity;

public interface IIdentityService
{
    public RoleType CurrentRole { get; }
    public bool IsAuthenticated { get; }
    public void SetUser(RoleType role);
    public void ClearUser();
    public bool IsInRole(params RoleType[] roles);
}