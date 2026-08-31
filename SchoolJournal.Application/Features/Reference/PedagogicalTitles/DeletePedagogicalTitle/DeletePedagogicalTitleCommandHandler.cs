using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.DeletePedagogicalTitle;

public sealed class DeletePedagogicalTitleCommandHandler(
    IPedagogicalTitleRepository pedagogicalTitleRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeletePedagogicalTitleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeletePedagogicalTitleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingTitle = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);

        if (existingTitle is null || existingTitle.IsDeleted)
        {
            return Error.NotFound(
                code: "PedagogicalTitle.NotFound",
                description: "Педагогічне звання не знайдено або вже видалено.");
        }

        auditContext.TrackOldState(existingTitle);

        var deletedOldState = await pedagogicalTitleRepository.DeleteAsync(request.TitleId, cancellationToken).ConfigureAwait(false);

        if (deletedOldState is null)
        {
            return Error.Conflict(
                code: "PedagogicalTitle.DeleteFailed",
                description: "Не вдалося видалити звання. Можливо, воно було видалено іншим користувачем.");
        }

        var newState = await pedagogicalTitleRepository.GetByIdAsync(request.TitleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}