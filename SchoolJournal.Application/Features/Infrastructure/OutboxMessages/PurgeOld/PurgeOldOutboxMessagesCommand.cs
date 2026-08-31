using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.PurgeOld;

public sealed record PurgeOldOutboxMessagesCommand(int OlderThanDays) : IRequest<ErrorOr<int>>;