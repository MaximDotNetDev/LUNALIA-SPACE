using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Semester.RestoreSemester;

public sealed class RestoreSemesterCommandValidator : AbstractValidator<RestoreSemesterCommand>
{
    public RestoreSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Ідентифікатор семестру є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.")
            .Must(BeValidBase64).WithMessage("Версія запису має бути у валідному форматі Base64.");
    }

    private static bool BeValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}