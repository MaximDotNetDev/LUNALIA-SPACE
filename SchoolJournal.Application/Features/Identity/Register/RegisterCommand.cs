using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Enums.Identity;

namespace SchoolJournal.Application.Features.Identity.Register;

public sealed record RegisterCommand(
    string Login,
    string Password,
    RoleType Role
) : IRequest<ErrorOr<Guid>>;