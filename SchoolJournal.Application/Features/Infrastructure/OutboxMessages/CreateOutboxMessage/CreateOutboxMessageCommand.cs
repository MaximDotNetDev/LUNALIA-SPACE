using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.CreateOutboxMessage;

public sealed record CreateOutboxMessageCommand(
    string Type,
    string Content
) : IRequest<ErrorOr<Guid>>;