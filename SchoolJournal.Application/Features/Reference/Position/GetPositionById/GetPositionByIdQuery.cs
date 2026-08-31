using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Positions;

namespace SchoolJournal.Application.Features.Reference.Position.GetPositionById;

public sealed record GetPositionByIdQuery(Guid PositionId) : IRequest<ErrorOr<PositionResponse>>;