using FluentValidation;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.UpdateSchoolClass;

public sealed class UpdateSchoolClassCommandValidator : AbstractValidator<UpdateSchoolClassCommand>
{
    public UpdateSchoolClassCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ID класу обов'язковий.");

        RuleFor(x => x.ClassName)
            .NotEmpty().WithMessage("Назва класу не може бути порожньою.")
            .MaximumLength(10).WithMessage("Назва класу не може перевищувати 10 символів.")
            .Must(name => name == name.Trim()).WithMessage("Назва не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.GradeLevel)
            .InclusiveBetween(1, 12).WithMessage("Рівень класу має бути від 1 до 12.");

        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Навчальний рік обов'язковий.")
            .MaximumLength(20).WithMessage("Навчальний рік занадто довгий.");

        RuleFor(x => x.HomeroomTeacherId)
            .NotEmpty().WithMessage("ID класного керівника обов'язковий.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) обов'язкова для оновлення.")
            .Must(BeAValidBase64).WithMessage("Неправильний формат RowVersion.");
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}