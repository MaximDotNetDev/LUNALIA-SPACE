using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkFailed;

public sealed class MarkOutboxMessageFailedCommandHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<MarkOutboxMessageFailedCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(MarkOutboxMessageFailedCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateTimeOffset? processedOnUtc = request.StopRetrying ? DateTimeOffset.UtcNow : null;

        var isUpdated = await outboxMessageRepository.UpdateStatusAsync(
            request.Id,
            processedOnUtc,
            request.ErrorMessage,
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