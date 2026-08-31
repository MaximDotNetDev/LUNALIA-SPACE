using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.GradeType.CreateGradeType;

public sealed class CreateGradeTypeCommandValidator : AbstractValidator<CreateGradeTypeCommand>
{
    public CreateGradeTypeCommandValidator()
    {
        RuleFor(x => x.TypeName)
            .NotEmpty().WithMessage("Назва типу оцінки не може бути порожньою.")
            .MaximumLength(50).WithMessage("Назва типу оцінки не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва типу оцінки не повинна містити пробіли на початку або в кінці.");
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