using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Position.DeletePosition;

public sealed class DeletePositionCommandHandler(
    IPositionRepository positionRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeletePositionCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await positionRepository.GetByIdAsync(request.PositionId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "Position.NotFound",
                description: "Посаду не знайдено або вона вже була видалена.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await positionRepository.DeleteAsync(request.PositionId, cancellationToken).ConfigureAwait(false);

        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}