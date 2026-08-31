using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.AssignStudentToSubgroup;

public sealed class AssignStudentToSubgroupCommandValidator : AbstractValidator<AssignStudentToSubgroupCommand>
{
    public AssignStudentToSubgroupCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор студента обов'язковий.");

        RuleFor(x => x.SubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор підгрупи обов'язковий.");
    }
}