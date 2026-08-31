using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetSubstitutionsByAssignmentId;

public sealed class GetSubstitutionsByAssignmentIdQueryValidator : AbstractValidator<GetSubstitutionsByAssignmentIdQuery>
{
    public GetSubstitutionsByAssignmentIdQueryValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");
    }
}