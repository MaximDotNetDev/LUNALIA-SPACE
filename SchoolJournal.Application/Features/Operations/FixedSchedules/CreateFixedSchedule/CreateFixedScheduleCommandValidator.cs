using FluentValidation;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.CreateFixedSchedule;

public sealed class CreateFixedScheduleCommandValidator : AbstractValidator<CreateFixedScheduleCommand>
{
    public CreateFixedScheduleCommandValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("Вказано некоректний день тижня.")
            .Must(BeAValidSchoolDay).WithMessage("День тижня має бути в межах від понеділка до неділі (1-7).");

        RuleFor(x => x.PeriodId)
            .NotEmpty().WithMessage("Ідентифікатор періоду (дзвінка) є обов'язковим.");

        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення (TeachingAssignment) є обов'язковим.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Ідентифікатор кабінету є обов'язковим.");
    }

    private static bool BeAValidSchoolDay(SchoolDayOfWeek day)
    {
        var dayValue = (int)day;
        return dayValue is >= 1 and <= 7;
    }
}