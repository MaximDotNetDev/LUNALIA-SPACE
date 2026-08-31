using FluentValidation;

namespace SchoolJournal.Application.Features.Identity.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Login)
                    .NotEmpty().WithMessage("Логін є обов'язковим.")
                    .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Логін містить недопустимі символи.")
                    .MaximumLength(33).WithMessage("Логін не може перевищувати 33 символів.");

        RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Пароль є обов'язковим.")
                    .MinimumLength(5).WithMessage("Пароль має бути не менше 5 символів.")
                    .MaximumLength(33).WithMessage("Пароль не може перевищувати 33 символів.");

        RuleFor(x => x.DeviceIdentifier)
            .NotEmpty().WithMessage("Ідентифікатор пристрою є обов'язковим.")
            .MaximumLength(100).WithMessage("Ідентифікатор пристрою не може перевищувати 100 символів.");
    }
}