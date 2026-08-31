using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;

namespace SchoolJournal.Application.Features.Infrastructure.SystemSettings.GetSystemSettings;

public sealed record GetSystemSettingsQuery : IRequest<ErrorOr<SystemSettingsResponse>>;