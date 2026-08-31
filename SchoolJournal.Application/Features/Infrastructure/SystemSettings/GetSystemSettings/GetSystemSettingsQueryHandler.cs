using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.SystemSettings;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.SystemSettings.GetSystemSettings;

public sealed class GetSystemSettingsQueryHandler(ISystemSettingRepository systemSettingRepository)
    : IRequestHandler<GetSystemSettingsQuery, ErrorOr<SystemSettingsResponse>>
{
    public async Task<ErrorOr<SystemSettingsResponse>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = await systemSettingRepository.GetAsync(cancellationToken).ConfigureAwait(false);

        if (settings is null)
        {
            return Error.NotFound(
                code: "SystemSettings.NotFound",
                description: "Системні налаштування ще не створені.");
        }

        return new SystemSettingsResponse(
            settings.SchoolName,
            settings.AcademicYear,
            settings.PrincipalName,
            Convert.ToBase64String(settings.RowVersion.ToArray())
        );
    }
}