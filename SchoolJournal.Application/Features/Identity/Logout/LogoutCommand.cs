using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Identity.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<ErrorOr<Success>>;