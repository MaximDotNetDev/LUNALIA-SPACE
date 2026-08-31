using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.RestoreStudentParent;

public sealed class RestoreStudentParentCommandValidator : AbstractValidator<RestoreStudentParentCommand>
{
    public RestoreStudentParentCommandValidator()
    {
        RuleFor(x => x.StudentParentId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку є обов'язковим.");
    }
}