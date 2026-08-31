using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.UpdateStudentParentRole;

public sealed class UpdateStudentParentRoleCommandValidator : AbstractValidator<UpdateStudentParentRoleCommand>
{
    public UpdateStudentParentRoleCommandValidator()
    {
        RuleFor(x => x.StudentParentId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку є обов'язковим.");

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