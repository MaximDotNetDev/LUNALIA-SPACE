using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkFailed;

public sealed record MarkOutboxMessageFailedCommand(
    Guid Id,
    string ErrorMessage,
    bool StopRetrying
) : IRequest<ErrorOr<Success>>;