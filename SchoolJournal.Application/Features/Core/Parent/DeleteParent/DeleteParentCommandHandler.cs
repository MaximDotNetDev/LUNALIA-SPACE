using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.DeleteParent;

public sealed class DeleteParentCommandHandler(
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteParentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteParentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);

        var deletedParent = await parentRepository.DeleteAsync(
            request.ParentId,
            rowVersion,
            cancellationToken).ConfigureAwait(false);

        if (deletedParent is null)
        {
            var exists = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);

            if (exists is null || exists.IsDeleted)
            {
                return Error.NotFound(
                    code: "Parent.NotFound",
                    description: "Профіль не знайдено або вже видалено.");
            }

            return Error.Conflict(
                code: "Parent.ConcurrencyConflict",
                description: "Не вдалося видалити. Запис був змінений іншим користувачем.");
        }

        auditContext.TrackOldState(deletedParent);

        return Result.Success;
    }
}