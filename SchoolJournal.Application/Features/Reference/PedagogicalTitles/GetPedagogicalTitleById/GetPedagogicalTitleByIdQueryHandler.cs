using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.PedagogicalTitles;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.GetPedagogicalTitleById;

public sealed class GetPedagogicalTitleByIdQueryHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository)
    : IRequestHandler<GetPedagogicalTitleByIdQuery, ErrorOr<PedagogicalTitleResponse>>
{
    public async Task<ErrorOr<PedagogicalTitleResponse>> Handle(GetPedagogicalTitleByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var title = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);

        if (title is null)
        {
            return Error.NotFound(
                code: "PedagogicalTitle.NotFound",
                description: "Педагогічне звання не знайдено.");
        }

        return title.Adapt<PedagogicalTitleResponse>();
    }
}