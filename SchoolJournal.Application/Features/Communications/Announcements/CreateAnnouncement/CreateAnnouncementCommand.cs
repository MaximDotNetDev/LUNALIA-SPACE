using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Communications.Announcements.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    string Title,
    string Content,
    Guid AuthorId,
    DateTimeOffset? ExpirationDate
) : IRequest<ErrorOr<Guid>>;