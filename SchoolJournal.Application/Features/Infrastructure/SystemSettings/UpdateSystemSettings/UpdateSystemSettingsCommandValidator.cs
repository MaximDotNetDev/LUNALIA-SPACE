using FluentValidation;

namespace SchoolJournal.Application.Features.Infrastructure.SystemSettings.UpdateSystemSettings;

public sealed class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
{
    public UpdateSystemSettingsCommandValidator()
    {
        RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("Назва школи є обов'язковою.")
            .MaximumLength(200).WithMessage("Назва школи не може перевищувати 200 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва школи не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Академічний рік є обов'язковим.")
            .MaximumLength(20).WithMessage("Академічний рік не може перевищувати 20 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Академічний рік не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.PrincipalName)
            .MaximumLength(100).WithMessage("Ім'я директора не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces!).When(x => !string.IsNullOrEmpty(x.PrincipalName))
            .WithMessage("Ім'я директора не повинно містити пробіли на початку або в кінці.");

        RuleFor(x => x.RowVersionBase64)
            .Must(BeValidBase64!).When(x => !string.IsNullOrWhiteSpace(x.RowVersionBase64))
            .WithMessage("Версія запису має бути у валідному форматі Base64.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return value.Trim().Length > 0 && value == value.Trim();
    }

    private static bool BeValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return true;
        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}