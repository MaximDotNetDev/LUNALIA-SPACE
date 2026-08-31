using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.DeleteFixedSchedule;

public sealed class DeleteFixedScheduleCommandValidator : AbstractValidator<DeleteFixedScheduleCommand>
{
    public DeleteFixedScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty().WithMessage("Ідентифікатор розкладу є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("RowVersion є обов'язковим для видалення.")
            .Must(BeAValidBase64).WithMessage("RowVersion має бути у коректному форматі Base64.");
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