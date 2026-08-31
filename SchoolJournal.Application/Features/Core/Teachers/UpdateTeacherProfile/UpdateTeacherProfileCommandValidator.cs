using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherProfile;

public sealed class UpdateTeacherProfileCommandValidator : AbstractValidator<UpdateTeacherProfileCommand>
{
    public UpdateTeacherProfileCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Прізвище є обов'язковим.")
            .MaximumLength(50)
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Прізвище не повинно містити пробіли на початку або в кінці.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ім'я є обов'язковим.")
            .MaximumLength(50)
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Ім'я не повинно містити пробіли на початку або в кінці.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Номер телефону задовгий.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних (RowVersion) є обов'язковою для оновлення.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
        => !string.IsNullOrEmpty(name) && name == name.Trim();
}