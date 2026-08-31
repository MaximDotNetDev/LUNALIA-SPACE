using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetTeacherSubstitutionById;

public sealed class GetTeacherSubstitutionByIdQueryValidator : AbstractValidator<GetTeacherSubstitutionByIdQuery>
{
    public GetTeacherSubstitutionByIdQueryValidator()
    {
        RuleFor(x => x.SubstitutionId)
            .NotEmpty().WithMessage("Ідентифікатор заміни є обов'язковим.");
    }
}