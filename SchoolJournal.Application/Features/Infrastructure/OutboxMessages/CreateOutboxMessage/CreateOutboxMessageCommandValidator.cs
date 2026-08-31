using System.Text.Json;
using FluentValidation;

namespace SchoolJournal.Application.Features.Infrastructure.OutboxMessages.CreateOutboxMessage;

public sealed class CreateOutboxMessageCommandValidator : AbstractValidator<CreateOutboxMessageCommand>
{
    public CreateOutboxMessageCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Тип повідомлення не може бути порожнім.")
            .MaximumLength(50).WithMessage("Тип повідомлення не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Тип повідомлення не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Контент повідомлення не може бути порожнім.")
            .Must(BeValidJson).WithMessage("Контент повідомлення має бути валідним JSON.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.Trim().Length > 0 && value == value.Trim();
    }

    private static bool BeValidJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}