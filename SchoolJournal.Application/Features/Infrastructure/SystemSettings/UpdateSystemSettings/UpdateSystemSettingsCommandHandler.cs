using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.SystemSettings.UpdateSystemSettings;

public sealed class UpdateSystemSettingsCommandHandler(
    ISystemSettingRepository systemSettingRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateSystemSettingsCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentUserId = currentUserService.GetUserId();
        if (currentUserId == Guid.Empty)
        {
            return Error.Unauthorized(
                code: "SystemSettings.Unauthorized",
                description: "Не вдалося ідентифікувати користувача.");
        }

        var oldState = await systemSettingRepository.GetAsync(cancellationToken).ConfigureAwait(false);

        if (oldState is not null && string.IsNullOrWhiteSpace(request.RowVersionBase64))
        {
            return Error.Validation(
                code: "SystemSettings.RowVersionRequired",
                description: "Налаштування вже існують. Для їх оновлення необхідно передати поточну версію (RowVersionBase64).");
        }

        if (oldState is not null)
        {
            auditContext.TrackOldState(oldState);
        }

        var systemSetting = new SystemSetting
        {
            SettingId = oldState?.SettingId ?? Guid.NewGuid(),
            SettingKey = 1,
            SchoolName = request.SchoolName,
            AcademicYear = request.AcademicYear,
            PrincipalName = request.PrincipalName,
            UpdatedByUserId = currentUserId,
            CreatedAt = oldState?.CreatedAt ?? DateTimeOffset.UtcNow,
            UpdatedAt = oldState is not null ? DateTimeOffset.UtcNow : null,
            RowVersion = string.IsNullOrWhiteSpace(request.RowVersionBase64)
                ? []
                : Convert.FromBase64String(request.RowVersionBase64)
        };

        var newStateFromDb = await systemSettingRepository.UpsertAsync(systemSetting, cancellationToken).ConfigureAwait(false);

        if (newStateFromDb is null)
        {
            return Error.Conflict(
                code: "SystemSettings.ConcurrencyConflict",
                description: "Дані налаштувань були змінені іншим адміністратором. Оновіть сторінку та спробуйте знову.");
        }

        auditContext.TrackNewState(newStateFromDb);

        return Result.Success;
    }
}