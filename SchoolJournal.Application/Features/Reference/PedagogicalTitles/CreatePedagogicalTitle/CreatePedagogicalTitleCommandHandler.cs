using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.CreatePedagogicalTitle;

public sealed class CreatePedagogicalTitleCommandHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreatePedagogicalTitleCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreatePedagogicalTitleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await pedagogicalTitleRepository.ExistsByNameAsync(request.TitleName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "PedagogicalTitle.NameConflict",
                description: $"Педагогічне звання з назвою '{request.TitleName}' вже існує.");
        }

        var title = new PedagogicalTitle
        {
            TitleName = request.TitleName
        };

        var titleId = await pedagogicalTitleRepository.AddAsync(title, cancellationToken).ConfigureAwait(false);

        var newState = await pedagogicalTitleRepository.GetByIdAsync(titleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return titleId;
    }
}