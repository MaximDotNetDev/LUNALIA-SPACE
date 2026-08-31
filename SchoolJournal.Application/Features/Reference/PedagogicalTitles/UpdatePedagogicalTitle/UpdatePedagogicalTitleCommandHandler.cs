using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.UpdatePedagogicalTitle;

public sealed class UpdatePedagogicalTitleCommandHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdatePedagogicalTitleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdatePedagogicalTitleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingTitle = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);
        if (existingTitle is null || existingTitle.IsDeleted)
        {
            return Error.NotFound(
                code: "PedagogicalTitle.NotFound",
                description: "Педагогічне звання не знайдено або воно було видалено.");
        }

        if (await pedagogicalTitleRepository.ExistsByNameExcludingIdAsync(request.TitleName, request.TitleId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "PedagogicalTitle.NameConflict",
                description: $"Педагогічне звання з назвою '{request.TitleName}' вже існує.");
        }

        auditContext.TrackOldState(existingTitle);

        var titleToUpdate = new PedagogicalTitle
        {
            TitleId = request.TitleId,
            TitleName = request.TitleName,
            IsDeleted = existingTitle.IsDeleted,
            CreatedAt = existingTitle.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updatedOldState = await pedagogicalTitleRepository.UpdateAsync(titleToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedOldState is null)
        {
            return Error.Conflict(
                code: "PedagogicalTitle.UpdateFailed",
                description: "Не вдалося оновити звання. Можливо, його було видалено або змінено.");
        }

        var newState = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}