using MediatR;
using SchoolJournal.Domain.Entities.Infrastructure;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetUnprocessed;

public sealed record GetUnprocessedOutboxMessagesQuery(int BatchSize = 50) : IRequest<IEnumerable<OutboxMessage>>;