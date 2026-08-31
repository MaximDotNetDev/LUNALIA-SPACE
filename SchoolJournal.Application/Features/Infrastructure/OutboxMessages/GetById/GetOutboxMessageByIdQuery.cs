using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Infrastructure.OutboxMessages;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.GetById;

public sealed record GetOutboxMessageByIdQuery(Guid Id) : IRequest<ErrorOr<OutboxMessageResponse>>;