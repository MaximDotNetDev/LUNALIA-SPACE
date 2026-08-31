using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Parent.UpdateParent;

public sealed class UpdateParentCommandHandler(
    IParentRepository parentRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateParentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateParentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingParent = await parentRepository.GetByIdAsync(request.ParentId, cancellationToken).ConfigureAwait(false);
        if (existingParent is null || existingParent.IsDeleted)
        {
            return Error.NotFound(
                code: "Parent.NotFound",
                description: "Профіль батьків не знайдено або його було видалено.");
        }

        var updateModel = new Domain.Entities.Core.Parent
        {
            ParentId = request.ParentId,
            LastName = request.LastName,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            Phone = request.Phone,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = Convert.FromBase64String(request.RowVersionBase64)
        };

        var oldState = await parentRepository.UpdateAsync(updateModel, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
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