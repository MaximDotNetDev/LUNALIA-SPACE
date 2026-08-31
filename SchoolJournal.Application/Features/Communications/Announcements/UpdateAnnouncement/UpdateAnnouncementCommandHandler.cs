using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Communications;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.UpdateAnnouncement;

public sealed class UpdateAnnouncementCommandHandler(
    IAnnouncementRepository announcementRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateAnnouncementCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
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

        var announcement = new Announcement
        {
            AnnouncementId = request.AnnouncementId,
            Title = request.Title,
            Content = request.Content,
            ExpirationDate = request.ExpirationDate,
            RowVersion = rowVersion
        };

        var oldState = await announcementRepository.UpdateAsync(announcement, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            // Якщо UPDATE повернув 0 рядків, це або RowVersion mismatch, або запис видалено/не існує
            var existing = await announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken).ConfigureAwait(false);

            if (existing is null || existing.IsDeleted)
                return Error.NotFound(description: "Оголошення не знайдено.");

            return Error.Conflict(description: "Дані були змінені іншим користувачем. Будь ласка, оновіть сторінку.");
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