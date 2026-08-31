using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Parent.UpdateParent;

public sealed class UpdateParentCommandValidator : AbstractValidator<UpdateParentCommand>
{
    public UpdateParentCommandValidator()
    {
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Ідентифікатор профілю є обов'язковим.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Прізвище є обов'язковим.")
            .MaximumLength(50).WithMessage("Прізвище не може перевищувати 50 символів.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ім'я є обов'язковим.")
            .MaximumLength(50).WithMessage("Ім'я не може перевищувати 50 символів.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("По батькові не може перевищувати 50 символів.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Телефон не може перевищувати 20 символів.");

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