using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.SoftDeleteAttendance;

public sealed class SoftDeleteAttendanceCommandValidator : AbstractValidator<SoftDeleteAttendanceCommand>
{
    public SoftDeleteAttendanceCommandValidator()
    {
        RuleFor(x => x.AttendanceId)
            .NotEmpty().WithMessage("Ідентифікатор запису відвідуваності є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою для конкурентного видалення.")
            .Must(BeValidBase64).WithMessage("Некоректний формат RowVersion. Очікується Base64-рядок.");
    }

    private static bool BeValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}