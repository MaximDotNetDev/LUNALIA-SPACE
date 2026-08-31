using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkProcessed;

public sealed record MarkOutboxMessageProcessedCommand(Guid Id) : IRequest<ErrorOr<Success>>;