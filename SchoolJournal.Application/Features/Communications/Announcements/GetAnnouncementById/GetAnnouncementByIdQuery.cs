using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Communications.Announcements;
using SchoolJournal.Domain.Entities.Communications.IRepositories;

namespace SchoolJournal.Application.Features.Communications.Announcements.GetAnnouncementById;

public sealed record GetAnnouncementByIdQuery(Guid Id) : IRequest<ErrorOr<AnnouncementResponse>>;

public sealed class GetAnnouncementByIdHandler(IAnnouncementRepository announcementRepository)
    : IRequestHandler<GetAnnouncementByIdQuery, ErrorOr<AnnouncementResponse>>
{
    public async Task<ErrorOr<AnnouncementResponse>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var x = await announcementRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (x is null || x.IsDeleted) return Error.NotFound(description: "Оголошення не знайдено.");

        return new AnnouncementResponse(
            x.AnnouncementId, x.Title, x.Content, x.AuthorId, x.DateCreated, x.ExpirationDate, x.IsActive, Convert.ToBase64String(x.RowVersion.ToArray()));
    }
}