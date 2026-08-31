using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;
using SchoolJournal.Domain.Entities.Infrastructure.IRepositories;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetList;

public sealed class GetOutboxMessagesListQueryHandler(
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<GetOutboxMessagesListQuery, PagedResponse<OutboxMessageResponse>>
{
    public async Task<PagedResponse<OutboxMessageResponse>> Handle(GetOutboxMessagesListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await outboxMessageRepository.GetPagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            request.Type,
            request.HasError,
            cancellationToken).ConfigureAwait(false);

        return new PagedResponse<OutboxMessageResponse>(
            items.Adapt<IEnumerable<OutboxMessageResponse>>(),
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}