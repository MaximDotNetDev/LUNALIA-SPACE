namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.RestorePedagogicalTitle;

using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

public sealed class RestorePedagogicalTitleCommandHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestorePedagogicalTitleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestorePedagogicalTitleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingTitle = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);

        if (existingTitle is null)
        {
            return Error.NotFound(
                code: "PedagogicalTitle.NotFound",
                description: "Педагогічне звання не знайдено.");
        }

        if (!existingTitle.IsDeleted)
        {
            return Error.Conflict(
                code: "PedagogicalTitle.NotDeleted",
                description: "Це педагогічне звання не знаходиться в архіві.");
        }

        auditContext.TrackOldState(existingTitle);

        var restoredOldState = await pedagogicalTitleRepository.RestoreAsync(request.TitleId, cancellationToken).ConfigureAwait(false);

        if (restoredOldState is null)
        {
            return Error.Conflict(
                code: "PedagogicalTitle.RestoreFailed",
                description: "Не вдалося відновити звання. Можливо, воно вже було відновлене іншим користувачем.");
        }

        var newState = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}