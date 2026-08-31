using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RemoveStudentFromSubgroup;

public sealed class RemoveStudentFromSubgroupCommandValidator : AbstractValidator<RemoveStudentFromSubgroupCommand>
{
    public RemoveStudentFromSubgroupCommandValidator()
    {
        RuleFor(x => x.StudentSubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку обов'язковий.");
    }
}