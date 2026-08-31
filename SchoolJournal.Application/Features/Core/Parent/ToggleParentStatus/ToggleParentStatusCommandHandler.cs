using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.ToggleParentStatus;

public sealed class ToggleParentStatusCommandHandler(
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<ToggleParentStatusCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ToggleParentStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await parentRepository.ToggleStatusAsync(
            request.ParentId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            var exists = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
            if (exists is null || exists.IsDeleted)
            {
                return Error.NotFound(
                    code: "Parent.NotFound",
                    description: "Профіль не знайдено або його було видалено.");
            }

            return Error.Conflict(
                code: "Parent.ConcurrencyConflict",
                description: "Статус був змінений іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        var newState = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            var trackOldMethod = auditContext.GetType().GetMethod("TrackOldState");
            trackOldMethod?.Invoke(auditContext, [oldState]);

            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}