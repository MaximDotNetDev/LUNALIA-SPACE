using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Position.DeletePosition;

public sealed record DeletePositionCommand(Guid PositionId) : IRequest<ErrorOr<Success>>;