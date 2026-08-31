using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.RemoveParentFromStudent;

public sealed class RemoveParentFromStudentCommandValidator : AbstractValidator<RemoveParentFromStudentCommand>
{
    public RemoveParentFromStudentCommandValidator()
    {
        RuleFor(x => x.StudentParentId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку є обов'язковим.");
    }
}