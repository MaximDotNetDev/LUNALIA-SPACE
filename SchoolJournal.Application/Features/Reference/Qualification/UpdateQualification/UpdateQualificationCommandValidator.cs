using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Qualification.UpdateQualification;

public sealed class UpdateQualificationCommandValidator : AbstractValidator<UpdateQualificationCommand>
{
    public UpdateQualificationCommandValidator()
    {
        RuleFor(x => x.QualificationId)
            .NotEmpty().WithMessage("ID кваліфікації є обов'язковим.");

        RuleFor(x => x.QualificationName)
            .NotEmpty().WithMessage("Назва кваліфікації є обов'язковою.")
            .MaximumLength(100).WithMessage("Назва кваліфікації не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва кваліфікації не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64String).WithMessage("Версія рядка має бути у правильному форматі Base64.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Trim().Length > 0 && name == name.Trim();
    }

    private static bool BeAValidBase64String(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}