using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Infrastructure;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.CreateOutboxMessage;

public sealed class CreateOutboxMessageCommandHandler(
    IOutboxMessageRepository outboxMessageRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateOutboxMessageCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateOutboxMessageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();

        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(
                code: "Outbox.Unauthorized",
                description: "Не вдалося ідентифікувати користувача для створення Outbox-повідомлення.");
        }

        var outboxMessage = new OutboxMessage
        {
            CreatedByUserId = userId,
            Type = request.Type,
            Content = request.Content,
            OccurredOnUtc = DateTimeOffset.UtcNow
        };

        var outboxMessageId = await outboxMessageRepository.AddAsync(outboxMessage, cancellationToken).ConfigureAwait(false);

        return outboxMessageId;
    }
}