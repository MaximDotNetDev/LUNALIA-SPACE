using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.CreateBellSchedule;

public sealed class CreateBellScheduleCommandValidator : AbstractValidator<CreateBellScheduleCommand>
{
    public CreateBellScheduleCommandValidator()
    {
        RuleFor(x => x.LessonNumber)
            .GreaterThan(0).WithMessage("Номер уроку має бути більшим за 0.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Час початку уроку є обов'язковим.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Час завершення уроку є обов'язковим.")
            .GreaterThan(x => x.StartTime).WithMessage("Час завершення уроку має бути пізнішим за час початку.");
    }
}