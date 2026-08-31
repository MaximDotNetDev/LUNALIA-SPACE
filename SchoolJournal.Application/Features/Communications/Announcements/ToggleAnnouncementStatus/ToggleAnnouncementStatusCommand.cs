using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Communications.Announcements.ToggleAnnouncementStatus;

public sealed record ToggleAnnouncementStatusCommand(
    Guid AnnouncementId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;