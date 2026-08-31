using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Positions;

namespace SchoolJournal.Application.Features.Reference.Position.GetPositionsList;

public sealed record GetPositionsListQuery(PageRequest PageRequest) : IRequest<PagedResponse<PositionResponse>>;