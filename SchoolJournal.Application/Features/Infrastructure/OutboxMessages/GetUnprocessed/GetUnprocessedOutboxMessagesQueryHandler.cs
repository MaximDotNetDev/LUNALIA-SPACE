using MediatR;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetUnprocessed;

public sealed class GetUnprocessedOutboxMessagesQueryHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<GetUnprocessedOutboxMessagesQuery, IEnumerable<OutboxMessage>>
{
    public async Task<IEnumerable<OutboxMessage>> Handle(GetUnprocessedOutboxMessagesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await outboxMessageRepository
            .GetUnprocessedAsync(request.BatchSize, cancellationToken)
            .ConfigureAwait(false);
    }
}