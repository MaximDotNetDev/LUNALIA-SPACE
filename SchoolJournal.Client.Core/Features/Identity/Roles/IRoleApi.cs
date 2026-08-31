using Refit;
using SchoolJournal.Contracts.DTOs.Identity.Roles;

namespace SchoolJournal.Client.Core.Features.Identity.Roles;

public interface IRoleApi
{
    [Get("/api/roles")]
    public Task<IApiResponse<IEnumerable<RoleResponse>>> GetRolesAsync(CancellationToken ct = default);
}