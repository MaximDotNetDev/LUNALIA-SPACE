using MediatR;
using SchoolJournal.Contracts.DTOs.Identity.Roles;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using ContractsRoleType = SchoolJournal.Contracts.Enums.Identity.RoleType;

namespace SchoolJournal.Application.Features.Identity.Role.GetRoles;

public sealed class GetRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetRolesQuery, IEnumerable<RoleResponse>>
{
    public async Task<IEnumerable<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return roles.Select(r => new RoleResponse(
                    r.RoleId,
                    (ContractsRoleType)r.RoleName,
                    r.Description
                ));
    }
}