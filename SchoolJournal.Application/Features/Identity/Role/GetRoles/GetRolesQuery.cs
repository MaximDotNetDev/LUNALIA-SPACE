using MediatR;
using SchoolJournal.Contracts.DTOs.Identity.Roles;

namespace SchoolJournal.Application.Features.Identity.Role.GetRoles;

public sealed record GetRolesQuery() : IRequest<IEnumerable<RoleResponse>>;