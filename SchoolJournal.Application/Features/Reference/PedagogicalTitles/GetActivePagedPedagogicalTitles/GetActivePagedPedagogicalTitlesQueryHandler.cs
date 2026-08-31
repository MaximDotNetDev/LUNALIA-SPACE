using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePagedPedagogicalTitles;

public sealed class GetActivePagedPedagogicalTitlesQueryHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository)
    : IRequestHandler<GetActivePagedPedagogicalTitlesQuery, ErrorOr<PagedResponse<PedagogicalTitleResponse>>>
{
    public async Task<ErrorOr<PagedResponse<PedagogicalTitleResponse>>> Handle(GetActivePagedPedagogicalTitlesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await pedagogicalTitleRepository.GetActivePagedAsync(
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