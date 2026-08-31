using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommandValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceCommandValidator()
    {
        RuleFor(x => x.AttendanceId)
            .NotEmpty().WithMessage("Ідентифікатор запису відвідуваності є обов'язковим.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Статус відвідуваності не може бути порожнім.")
            .MaximumLength(20).WithMessage("Статус не може перевищувати 20 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Статус не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Comment)
            .MaximumLength(255).WithMessage("Коментар не може перевищувати 255 символів.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою для оптимістичного блокування.")
            .Must(BeValidBase64).WithMessage("Некоректний формат RowVersion. Очікується Base64-рядок.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return false;
        }

        return status.Trim().Length > 0 && status == status.Trim();
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