using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.CreateTeacher;

public sealed class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Прізвище є обов'язковим.")
            .MaximumLength(50).WithMessage("Прізвище не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Прізвище не повинно містити пробіли на початку або в кінці.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ім'я є обов'язковим.")
            .MaximumLength(50).WithMessage("Ім'я не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Ім'я не повинно містити пробіли на початку або в кінці.");

        RuleFor(x => x.Workload)
            .InclusiveBetween(0, 2.0m).WithMessage("Навантаження має бути в межах від 0 до 2.0.")
            .When(x => x.Workload.HasValue);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Номер телефону задовгий.");

        RuleFor(x => x.PositionId).NotEmpty().WithMessage("Посада є обов'язковою.");
        RuleFor(x => x.QualificationId).NotEmpty().WithMessage("Кваліфікація є обов'язковою.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
        => !string.IsNullOrEmpty(name) && name.Trim().Length > 0 && name == name.Trim();
}