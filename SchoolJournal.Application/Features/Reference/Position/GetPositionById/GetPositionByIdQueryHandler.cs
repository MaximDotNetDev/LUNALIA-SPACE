using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Positions;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Position.GetPositionById;

public sealed class GetPositionByIdQueryHandler(IPositionRepository positionRepository)
    : IRequestHandler<GetPositionByIdQuery, ErrorOr<PositionResponse>>
{
    public async Task<ErrorOr<PositionResponse>> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var position = await positionRepository.GetByIdAsync(request.PositionId, cancellationToken).ConfigureAwait(false);

        if (position is null || position.IsDeleted)
        {
            return Error.NotFound(
                code: "Position.NotFound",
                description: "Посаду не знайдено або вона була видалена.");
        }

        return new PositionResponse(
            position.PositionId,
            position.PositionName
        );
    }
}