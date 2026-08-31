using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.LessonType.UpdateLessonType;

public sealed class UpdateLessonTypeCommandValidator : AbstractValidator<UpdateLessonTypeCommand>
{
    public UpdateLessonTypeCommandValidator()
    {
        RuleFor(x => x.LessonTypeId)
            .NotEmpty().WithMessage("ID типу уроку є обов'язковим.");

        RuleFor(x => x.TypeName)
            .NotEmpty().WithMessage("Назва типу уроку не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва типу уроку має містити щонайменше 2 символи.")
            .MaximumLength(50).WithMessage("Назва типу уроку не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва типу уроку не повинна містити пробіли на початку або в кінці.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.Trim().Length > 0 && name == name.Trim();
    }
}