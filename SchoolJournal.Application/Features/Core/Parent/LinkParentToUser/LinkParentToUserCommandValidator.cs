using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Parent.LinkParentToUser;

public sealed class LinkParentToUserCommandValidator : AbstractValidator<LinkParentToUserCommand>
{
    public LinkParentToUserCommandValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Ідентифікатор профілю є обов'язковим.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Ідентифікатор користувача є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Невірний формат версії запису.");
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return false;
        }

        Span<byte> buffer = new byte[base64String.Length];
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}