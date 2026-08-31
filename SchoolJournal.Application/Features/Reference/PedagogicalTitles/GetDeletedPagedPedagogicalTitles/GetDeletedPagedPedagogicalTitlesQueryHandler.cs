using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetDeletedPagedPedagogicalTitles;

public sealed class GetDeletedPagedPedagogicalTitlesQueryHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository)
    : IRequestHandler<GetDeletedPagedPedagogicalTitlesQuery, ErrorOr<PagedResponse<PedagogicalTitleResponse>>>
{
    public async Task<ErrorOr<PagedResponse<PedagogicalTitleResponse>>> Handle(GetDeletedPagedPedagogicalTitlesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await pedagogicalTitleRepository.GetDeletedPagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        return new PagedResponse<PedagogicalTitleResponse>(
            items.Adapt<IEnumerable<PedagogicalTitleResponse>>(),
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}