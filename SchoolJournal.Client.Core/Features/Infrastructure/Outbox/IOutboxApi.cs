using Refit;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

namespace SchoolJournal.Client.Core.Features.Infrastructure.Outbox;

public interface IOutboxApi
{
    [Get("/api/outbox")]
    public Task<IApiResponse<PagedResponse<OutboxMessageResponse>>> GetOutboxMessagesAsync(
        [Query] int pageNumber,
        [Query] int pageSize,
        [Query] string? type = null,
        [Query] bool? hasError = null,
        CancellationToken ct = default);

    [Get("/api/outbox/{id}")]
    public Task<IApiResponse<OutboxMessageResponse>> GetOutboxMessageByIdAsync(Guid id, CancellationToken ct = default);

    [Put("/api/outbox/{id}/process")]
    public Task<IApiResponse> MarkAsProcessedAsync(Guid id, CancellationToken ct = default);

    [Put("/api/outbox/{id}/fail")]
    public Task<IApiResponse> MarkAsFailedAsync(Guid id, [Body] MarkOutboxMessageFailedRequest request, CancellationToken ct = default);

    [Delete("/api/outbox/purge")]
    public Task<IApiResponse<object>> PurgeOldMessagesAsync([Body] PurgeOutboxMessagesRequest request, CancellationToken ct = default);
}