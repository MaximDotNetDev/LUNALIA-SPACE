using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.ToggleAnnouncementStatus;

public sealed class ToggleAnnouncementStatusCommandHandler(
    IAnnouncementRepository announcementRepository,
    IAuditContext auditContext)
    : IRequestHandler<ToggleAnnouncementStatusCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ToggleAnnouncementStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        }
        catch (FormatException)
        {
            return Error.Validation(description: "Некоректний формат RowVersion.");
        }

        var oldState = await announcementRepository.ToggleStatusAsync(
            request.AnnouncementId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            var existing = await announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken).ConfigureAwait(false);

            if (existing is null || existing.IsDeleted)
                return Error.NotFound(description: "Оголошення не знайдено.");

            return Error.Conflict(description: "Статус вже було змінено іншим користувачем.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}