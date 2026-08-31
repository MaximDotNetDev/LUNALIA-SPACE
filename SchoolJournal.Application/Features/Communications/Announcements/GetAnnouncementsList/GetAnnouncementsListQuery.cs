using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.GetAnnouncementsList;

public sealed record GetAnnouncementsListQuery(
    PageRequest PageRequest,
    string? SearchTerm = null,
    bool? IsActive = null,
    Guid? AuthorId = null) : IRequest<PagedResponse<AnnouncementResponse>>;

public sealed class GetAnnouncementsListHandler(IAnnouncementRepository announcementRepository)
    : IRequestHandler<GetAnnouncementsListQuery, PagedResponse<AnnouncementResponse>>
{
    public async Task<PagedResponse<AnnouncementResponse>> Handle(GetAnnouncementsListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, total) = await announcementRepository.GetAdminPagedAsync(
            request.PageRequest.Skip, request.PageRequest.PageSize, request.SearchTerm, request.IsActive, request.AuthorId, cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(x => new AnnouncementResponse(
            x.AnnouncementId, x.Title, x.Content, x.AuthorId, x.DateCreated, x.ExpirationDate, x.IsActive, Convert.ToBase64String(x.RowVersion.ToArray())));

        return new PagedResponse<AnnouncementResponse>(responseItems, request.PageRequest.PageNumber, request.PageRequest.PageSize, total);
    }
}