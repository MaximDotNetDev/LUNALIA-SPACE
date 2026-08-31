using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Semester.UpdateSemester;

public sealed class UpdateSemesterCommandValidator : AbstractValidator<UpdateSemesterCommand>
{
    public UpdateSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Ідентифікатор семестру є обов'язковим.");

        RuleFor(x => x.SemesterName)
                    .NotEmpty().WithMessage("Назва семестру не може бути порожньою.")
                    .MinimumLength(2).WithMessage("Назва семестру має містити щонайменше 2 символи.")
                    .MaximumLength(50).WithMessage("Назва семестру не може перевищувати 50 символів.")
                    .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва семестру не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата початку семестру є обов'язковою.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Дата закінчення семестру є обов'язковою.")
            .GreaterThan(x => x.StartDate).WithMessage("Дата закінчення має бути пізнішою за дату початку.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.")
            .Must(BeValidBase64).WithMessage("Версія запису має бути у валідному форматі Base64.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.Trim().Length > 0 && name == name.Trim();
    }

    private static bool BeValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}