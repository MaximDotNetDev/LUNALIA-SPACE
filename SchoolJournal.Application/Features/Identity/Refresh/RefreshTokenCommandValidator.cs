using FluentValidation;

namespace SchoolJournal.Application.Features.Identity.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Рефреш-токен є обов'язковим.");

        RuleFor(x => x.DeviceIdentifier)
            .MaximumLength(100).WithMessage("Ідентифікатор пристрою не може перевищувати 100 символів.");
    }
}