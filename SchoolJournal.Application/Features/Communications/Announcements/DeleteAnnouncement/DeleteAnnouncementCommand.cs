using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Communications.Announcements.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(
    Guid AnnouncementId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;