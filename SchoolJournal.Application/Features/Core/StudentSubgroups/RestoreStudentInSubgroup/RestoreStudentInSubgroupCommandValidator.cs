using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RestoreStudentInSubgroup;

public sealed class RestoreStudentInSubgroupCommandValidator : AbstractValidator<RestoreStudentInSubgroupCommand>
{
    public RestoreStudentInSubgroupCommandValidator()
    {
        RuleFor(x => x.StudentSubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку обов'язковий.");
    }
}