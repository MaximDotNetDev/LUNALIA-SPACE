using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Identity.Login;

namespace SchoolJournal.Application.Features.Identity.Login;

public sealed record LoginCommand(
    string Login,
    string Password,
    string? DeviceIdentifier) : IRequest<ErrorOr<TokenResponse>>;