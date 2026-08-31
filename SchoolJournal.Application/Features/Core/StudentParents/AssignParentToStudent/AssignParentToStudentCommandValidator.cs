using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.AssignParentToStudent;

public sealed class AssignParentToStudentCommandValidator : AbstractValidator<AssignParentToStudentCommand>
{
    public AssignParentToStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор учня є обов'язковим.");

        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Ідентифікатор батьків є обов'язковим.");

        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("Роль не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).When(x => !string.IsNullOrEmpty(x.Role))
            .WithMessage("Роль не повинна містити пробіли на початку або в кінці.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string? role)
    {
        if (string.IsNullOrEmpty(role))
        {
            return true;
        }

        return role.Trim().Length > 0 && role == role.Trim();
    }
}