using FluentValidation;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.CreateSchoolClass;

public sealed class CreateSchoolClassCommandValidator : AbstractValidator<CreateSchoolClassCommand>
{
    public CreateSchoolClassCommandValidator()
    {
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
    }
}