using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Lessons.RescheduleLesson;

public sealed class RescheduleLessonCommandValidator : AbstractValidator<RescheduleLessonCommand>
{
    public RescheduleLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");

        RuleFor(x => x.LessonDate)
            .NotEmpty().WithMessage("Нова дата уроку є обов'язковою.");

        RuleFor(x => x.PeriodId)
            .NotEmpty().WithMessage("Ідентифікатор періоду (дзвінка) є обов'язковим.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Ідентифікатор кабінету є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Токен конкурентності є обов'язковим.");
    }
}