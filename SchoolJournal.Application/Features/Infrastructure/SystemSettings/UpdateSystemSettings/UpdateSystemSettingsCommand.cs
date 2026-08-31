using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Infrastructure.SystemSettings.UpdateSystemSettings;

public sealed record UpdateSystemSettingsCommand(
    string SchoolName,
    string AcademicYear,
    string? PrincipalName,
    string? RowVersionBase64
) : IRequest<ErrorOr<Success>>;