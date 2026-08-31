using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.GetActiveSubstitutions;

public sealed class GetActiveSubstitutionsQueryValidator : AbstractValidator<GetActiveSubstitutionsQuery>
{
    public GetActiveSubstitutionsQueryValidator()
    {
    }
}