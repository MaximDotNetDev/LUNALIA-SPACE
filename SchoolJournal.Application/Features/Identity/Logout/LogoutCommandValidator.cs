using FluentValidation;

namespace SchoolJournal.Application.Features.Identity.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Рефреш-токен є обов'язковим для виходу із системи.");
    }
}