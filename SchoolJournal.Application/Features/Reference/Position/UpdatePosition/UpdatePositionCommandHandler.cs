using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Position.UpdatePosition;

public sealed class UpdatePositionCommandHandler(
    IPositionRepository positionRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdatePositionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await positionRepository.GetByIdAsync(request.PositionId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "Position.NotFound",
                description: "Посаду не знайдено або вона була видалена.");
        }

        auditContext.TrackOldState(oldState);

        if (string.Equals(oldState.PositionName, request.PositionName, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success;
        }

        if (await positionRepository.ExistsByNameExcludingIdAsync(request.PositionName, request.PositionId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Position.NameConflict",
                description: $"Інша посада з назвою '{request.PositionName}' вже існує.");
        }

        var positionToUpdate = oldState with { PositionName = request.PositionName };

        var newState = await positionRepository.UpdateAsync(positionToUpdate, cancellationToken).ConfigureAwait(false);

        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}