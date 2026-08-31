using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Communications;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler(
    IAnnouncementRepository announcementRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateAnnouncementCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId,
            ExpirationDate = request.ExpirationDate
        };

        var announcementId = await announcementRepository.AddAsync(announcement, cancellationToken).ConfigureAwait(false);

        var newState = await announcementRepository.GetByIdAsync(announcementId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return announcementId;
    }
}