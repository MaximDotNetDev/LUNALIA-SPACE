using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Communications.Announcements.UpdateAnnouncement;

public sealed record UpdateAnnouncementCommand(
    Guid AnnouncementId,
    string Title,
    string Content,
    DateTimeOffset? ExpirationDate,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;