using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByTeacherId;

public sealed class GetSubstitutionsByTeacherIdQueryValidator : AbstractValidator<GetSubstitutionsByTeacherIdQuery>
{
    public GetSubstitutionsByTeacherIdQueryValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");
    }
}