using FluentValidation;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.MarkFailed;

public sealed class MarkOutboxMessageFailedCommandValidator : AbstractValidator<MarkOutboxMessageFailedCommand>
{
    public MarkOutboxMessageFailedCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID повідомлення є обов'язковим.");

        RuleFor(x => x.ErrorMessage)
            .NotEmpty().WithMessage("Текст помилки не може бути порожнім.")
            .MaximumLength(4000).WithMessage("Текст помилки не може перевищувати 4000 символів.");
    }
}