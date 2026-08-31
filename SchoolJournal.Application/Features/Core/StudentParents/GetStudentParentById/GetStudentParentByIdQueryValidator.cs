using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentParents.GetStudentParentById;

public sealed class GetStudentParentByIdQueryValidator : AbstractValidator<GetStudentParentByIdQuery>
{
    public GetStudentParentByIdQueryValidator()
    {
        RuleFor(x => x.StudentParentId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку є обов'язковим.");
    }
}