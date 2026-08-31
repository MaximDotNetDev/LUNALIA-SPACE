using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandHandler(
    IAnnouncementRepository announcementRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteAnnouncementCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
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

        var oldState = await announcementRepository.DeleteAsync(
            request.AnnouncementId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            var existing = await announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken).ConfigureAwait(false);

            if (existing is null || existing.IsDeleted)
                return Error.NotFound(description: "Оголошення не знайдено або вже видалено.");

            return Error.Conflict(description: "Оголошення було змінено іншим користувачем. Видалення скасовано.");
        }

        auditContext.TrackOldState(oldState);

        return Result.Success;
    }
}