using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.GetActiveAnnouncements;

public sealed record GetActiveAnnouncementsQuery(PageRequest PageRequest) : IRequest<PagedResponse<AnnouncementResponse>>;

public sealed class GetActiveAnnouncementsHandler(IAnnouncementRepository announcementRepository)
    : IRequestHandler<GetActiveAnnouncementsQuery, PagedResponse<AnnouncementResponse>>
{
    public async Task<PagedResponse<AnnouncementResponse>> Handle(GetActiveAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, total) = await announcementRepository.GetActivePagedAsync(request.PageRequest.Skip, request.PageRequest.PageSize, cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(x => new AnnouncementResponse(
            x.AnnouncementId, x.Title, x.Content, x.AuthorId, x.DateCreated, x.ExpirationDate, x.IsActive, Convert.ToBase64String(x.RowVersion.ToArray())));

        return new PagedResponse<AnnouncementResponse>(responseItems, request.PageRequest.PageNumber, request.PageRequest.PageSize, total);
    }
}