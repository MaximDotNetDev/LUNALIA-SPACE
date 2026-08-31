using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetParentsByStudentId;

public sealed class GetParentsByStudentIdQueryValidator : AbstractValidator<GetParentsByStudentIdQuery>
{
    public GetParentsByStudentIdQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор учня є обов'язковим.");
    }
}