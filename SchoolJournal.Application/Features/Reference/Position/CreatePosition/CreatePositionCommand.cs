using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.Position.CreatePosition;

public sealed record CreatePositionCommand(string PositionName) : IRequest<ErrorOr<Guid>>;