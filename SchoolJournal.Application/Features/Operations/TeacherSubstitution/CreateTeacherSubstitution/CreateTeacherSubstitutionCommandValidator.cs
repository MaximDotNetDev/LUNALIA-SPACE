using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.CreateTeacherSubstitution;

public sealed class CreateTeacherSubstitutionCommandValidator : AbstractValidator<CreateTeacherSubstitutionCommand>
{
    public CreateTeacherSubstitutionCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.SubstituteTeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя на заміну є обов'язковим.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата початку заміни є обов'язковою.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Дата закінчення заміни є обов'язковою.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Дата закінчення має бути більшою або дорівнювати даті початку.");
    }
}