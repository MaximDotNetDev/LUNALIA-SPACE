using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Identity.Login;

namespace SchoolJournal.Application.Features.Identity.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? DeviceIdentifier) : IRequest<ErrorOr<TokenResponse>>;