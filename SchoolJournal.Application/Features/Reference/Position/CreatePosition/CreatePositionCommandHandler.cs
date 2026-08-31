using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Position.CreatePosition;

public sealed class CreatePositionCommandHandler(
    IPositionRepository positionRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreatePositionCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreatePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await positionRepository.ExistsByNameAsync(request.PositionName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Position.NameConflict",
                description: $"Посада з назвою '{request.PositionName}' вже існує.");
        }

        var position = new Domain.Entities.Reference.Position
        {
            PositionName = request.PositionName
        };

        var positionId = await positionRepository.AddAsync(position, cancellationToken).ConfigureAwait(false);

        var newState = await positionRepository.GetByIdAsync(positionId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return positionId;
    }
}