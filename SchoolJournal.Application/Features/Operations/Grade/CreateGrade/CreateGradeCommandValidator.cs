using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.CreateGrade;

public sealed class CreateGradeCommandValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Урок є обов'язковим.");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Студент є обов'язковим.");

        RuleFor(x => x.GradeTypeId)
            .NotEmpty().WithMessage("Тип оцінки є обов'язковим.");

        RuleFor(x => x.GradeValue)
            .NotEmpty().WithMessage("Значення оцінки не може бути порожнім.")
            .MaximumLength(3).WithMessage("Максимальна довжина значення оцінки — 3 символи.")
            .Must(BeAValidGradeValue).WithMessage("Оцінка повинна бути числом від 1 до 12, або 'Н', або 'хв', без пробілів на початку чи в кінці.");

        RuleFor(x => x.Comment)
            .MaximumLength(255).WithMessage("Коментар не може перевищувати 255 символів.");
    }

    private static bool BeAValidGradeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.Trim().Length == 0 || value != value.Trim())
        {
            return false;
        }

        if (value == "Н" || value == "хв")
        {
            return true;
        }

        if (int.TryParse(value, out int numericGrade))
        {
            return numericGrade is >= 1 and <= 12;
        }

        return false;
    }
}