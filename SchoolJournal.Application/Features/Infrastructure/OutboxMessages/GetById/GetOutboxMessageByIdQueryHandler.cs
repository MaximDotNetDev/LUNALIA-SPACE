using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetById;

public sealed class GetOutboxMessageByIdQueryHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<GetOutboxMessageByIdQuery, ErrorOr<OutboxMessageResponse>>
{
    public async Task<ErrorOr<OutboxMessageResponse>> Handle(GetOutboxMessageByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var message = await outboxMessageRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (message is null)
        {
            return Error.NotFound(
                code: "Outbox.NotFound",
                description: $"Повідомлення з ID {request.Id} не знайдено.");
        }

        return message.Adapt<OutboxMessageResponse>();
    }
}