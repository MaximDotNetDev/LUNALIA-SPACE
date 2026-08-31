using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Position.UpdatePosition;

public sealed record UpdatePositionCommand(
    Guid PositionId,
    string PositionName
) : IRequest<ErrorOr<Success>>;