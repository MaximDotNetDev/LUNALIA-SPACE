using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Position.CreatePosition;

public sealed class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.PositionName)
            .NotEmpty().WithMessage("Назва посади не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва посади має містити щонайменше 2 символи.")
            .MaximumLength(100).WithMessage("Назва посади не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва посади не повинна містити пробіли на початку або в кінці.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Trim().Length > 0 && name == name.Trim();
    }
}