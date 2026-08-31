using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.CreatePedagogicalTitle;

public sealed class CreatePedagogicalTitleCommandValidator : AbstractValidator<CreatePedagogicalTitleCommand>
{
    public CreatePedagogicalTitleCommandValidator()
    {
        RuleFor(x => x.TitleName)
            .NotEmpty().WithMessage("Назва педагогічного звання не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва має містити щонайменше 2 символи.")
            .MaximumLength(100).WithMessage("Назва не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва не повинна містити пробіли на початку або в кінці.");
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