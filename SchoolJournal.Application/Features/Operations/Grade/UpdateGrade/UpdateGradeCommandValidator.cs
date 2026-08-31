using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.UpdateGrade;

public sealed class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
{
    public UpdateGradeCommandValidator()
    {
        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Ідентифікатор оцінки є обов'язковим.");

        RuleFor(x => x.GradeTypeId)
            .NotEmpty().WithMessage("Тип оцінки є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою для запобігання конфліктів паралелізму.");

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

        if (value is "Н" or "хв")
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