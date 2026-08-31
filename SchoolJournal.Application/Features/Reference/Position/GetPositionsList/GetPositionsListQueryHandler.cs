using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Positions;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Position.GetPositionsList;

public sealed class GetPositionsListQueryHandler(IPositionRepository positionRepository)
    : IRequestHandler<GetPositionsListQuery, PagedResponse<PositionResponse>>
{
    public async Task<PagedResponse<PositionResponse>> Handle(GetPositionsListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await positionRepository.GetPagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(p => new PositionResponse(
            p.PositionId,
            p.PositionName));

        return new PagedResponse<PositionResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}