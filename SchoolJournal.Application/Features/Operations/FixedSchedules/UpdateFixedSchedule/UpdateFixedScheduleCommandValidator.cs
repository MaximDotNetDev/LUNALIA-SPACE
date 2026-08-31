using FluentValidation;
using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.UpdateFixedSchedule;

public sealed class UpdateFixedScheduleCommandValidator : AbstractValidator<UpdateFixedScheduleCommand>
{
    public UpdateFixedScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty().WithMessage("Ідентифікатор розкладу є обов'язковим.");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("Вказано некоректний день тижня.")
            .Must(BeAValidSchoolDay).WithMessage("День тижня має бути в межах від понеділка до неділі (1-7).");

        RuleFor(x => x.PeriodId)
            .NotEmpty().WithMessage("Ідентифікатор періоду (дзвінка) є обов'язковим.");

        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення (TeachingAssignment) є обов'язковим.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Ідентифікатор кабінету є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("RowVersion є обов'язковим для оновлення.")
            .Must(BeAValidBase64).WithMessage("RowVersion має бути у коректному форматі Base64.");
    }

    private static bool BeAValidSchoolDay(SchoolDayOfWeek day)
    {
        var dayValue = (int)day;
        return dayValue is >= 1 and <= 7;
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
        {
            return false;
        }

        var buffer = new Span<byte>(new byte[base64String.Length]);
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}