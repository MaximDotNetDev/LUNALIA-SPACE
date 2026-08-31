using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByStudent;

public sealed class GetGradesByStudentQueryValidator : AbstractValidator<GetGradesByStudentQuery>
{
    public GetGradesByStudentQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор студента є обов'язковим.");
    }
}