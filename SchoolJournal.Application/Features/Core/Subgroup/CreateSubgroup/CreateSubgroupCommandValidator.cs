using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subgroup.CreateSubgroup;

public sealed class CreateSubgroupCommandValidator : AbstractValidator<CreateSubgroupCommand>
{
    public CreateSubgroupCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Клас є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Предмет є обов'язковим.");

        RuleFor(x => x.SubgroupName)
            .NotEmpty().WithMessage("Назва підгрупи не може бути порожньою.")
            .MinimumLength(1).WithMessage("Назва підгрупи не може бути порожньою.")
            .MaximumLength(50).WithMessage("Назва підгрупи не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва підгрупи не повинна містити пробіли на початку або в кінці.");
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