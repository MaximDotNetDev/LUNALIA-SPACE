using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Identity.UpdateAccount;

public sealed record UpdateAccountCommand(
    Guid UserId,
    string Login,
    string? NewPassword
) : IRequest<ErrorOr<Success>>;