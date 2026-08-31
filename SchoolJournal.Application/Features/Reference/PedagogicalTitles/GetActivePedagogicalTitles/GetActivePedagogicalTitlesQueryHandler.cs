using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetActivePedagogicalTitles;

public sealed class GetActivePedagogicalTitlesQueryHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository)
    : IRequestHandler<GetActivePedagogicalTitlesQuery, ErrorOr<IEnumerable<PedagogicalTitleResponse>>>
{
    public async Task<ErrorOr<IEnumerable<PedagogicalTitleResponse>>> Handle(GetActivePedagogicalTitlesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var titles = await pedagogicalTitleRepository.GetActiveAsync(cancellationToken).ConfigureAwait(false);

        return titles.Adapt<IEnumerable<PedagogicalTitleResponse>>().ToList();
    }
}