using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Qualification.RestoreQualification;

public sealed class RestoreQualificationCommandValidator : AbstractValidator<RestoreQualificationCommand>
{
    public RestoreQualificationCommandValidator()
    {
        RuleFor(x => x.QualificationId)
            .NotEmpty().WithMessage("ID кваліфікації є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64String).WithMessage("Версія рядка має бути у правильному форматі Base64.");
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