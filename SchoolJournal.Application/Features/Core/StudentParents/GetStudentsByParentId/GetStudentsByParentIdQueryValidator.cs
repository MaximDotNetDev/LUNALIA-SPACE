using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentsByParentId;

public sealed class GetStudentsByParentIdQueryValidator : AbstractValidator<GetStudentsByParentIdQuery>
{
    public GetStudentsByParentIdQueryValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Ідентифікатор батьків є обов'язковим.");
    }
}