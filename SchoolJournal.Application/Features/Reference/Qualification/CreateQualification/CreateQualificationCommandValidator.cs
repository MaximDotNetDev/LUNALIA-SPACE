using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Qualification.CreateQualification;

public sealed class CreateQualificationCommandValidator : AbstractValidator<CreateQualificationCommand>
{
    public CreateQualificationCommandValidator()
    {
        RuleFor(x => x.QualificationName)
            .NotEmpty().WithMessage("Назва кваліфікації є обов'язковою.")
            .MaximumLength(100).WithMessage("Назва кваліфікації не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва кваліфікації не повинна містити пробіли на початку або в кінці.");
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