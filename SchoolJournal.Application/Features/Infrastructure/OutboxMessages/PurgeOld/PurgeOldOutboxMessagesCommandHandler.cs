using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.PurgeOld;

public sealed class PurgeOldOutboxMessagesCommandHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<PurgeOldOutboxMessagesCommand, ErrorOr<int>>
{
    public async Task<ErrorOr<int>> Handle(PurgeOldOutboxMessagesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var cutOffDate = DateTimeOffset.UtcNow.AddDays(-request.OlderThanDays);

        var deletedCount = await outboxMessageRepository
            .DeleteProcessedOlderThanAsync(cutOffDate, cancellationToken)
            .ConfigureAwait(false);

        return deletedCount;
    }
}