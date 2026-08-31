using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.LinkParentToUser;

public sealed class LinkParentToUserCommandHandler(
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<LinkParentToUserCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LinkParentToUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await parentRepository.LinkToUserAsync(
            request.ParentId,
            request.UserId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            var exists = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
            if (exists is null || exists.IsDeleted)
            {
                return Error.NotFound(
                    code: "Parent.NotFound",
                    description: "Профіль батьків не знайдено або його було видалено.");
            }

            return Error.Conflict(
                code: "Parent.ConcurrencyConflict",
                description: "Дані були змінені іншим користувачем. Будь ласка, оновіть сторінку та спробуйте ще раз.");
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