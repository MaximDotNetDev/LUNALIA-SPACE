using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkProcessed;

public sealed class MarkOutboxMessageProcessedCommandHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<MarkOutboxMessageProcessedCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(MarkOutboxMessageProcessedCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var isUpdated = await outboxMessageRepository.UpdateStatusAsync(
                    request.Id,
                    DateTimeOffset.UtcNow,
                    errorMessage: null,
                    cancellationToken).ConfigureAwait(false);

        if (!isUpdated)
        {
            return Error.NotFound(
                code: "Outbox.NotFoundOrAlreadyProcessed",
                description: $"Повідомлення з ID {request.Id} не знайдено, або воно вже було оброблено.");
        }

        return Result.Success;
    }
}