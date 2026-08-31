using FluentValidation;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.PurgeOld;

public sealed class PurgeOldOutboxMessagesCommandValidator : AbstractValidator<PurgeOldOutboxMessagesCommand>
{
    public PurgeOldOutboxMessagesCommandValidator()
    {
        RuleFor(x => x.OlderThanDays)
            .InclusiveBetween(1, 365)
            .WithMessage("Термін очищення має бути від 1 до 365 днів.");
    }
}