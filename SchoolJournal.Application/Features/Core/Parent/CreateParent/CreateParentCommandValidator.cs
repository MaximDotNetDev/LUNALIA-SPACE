using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Parent.CreateParent;

public sealed class CreateParentCommandValidator : AbstractValidator<CreateParentCommand>
{
    public CreateParentCommandValidator()
    {
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
    }
}