using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetList;

public sealed record GetOutboxMessagesListQuery(
    PageRequest PageRequest,
    string? Type = null,
    bool? HasError = null
) : IRequest<PagedResponse<OutboxMessageResponse>>;