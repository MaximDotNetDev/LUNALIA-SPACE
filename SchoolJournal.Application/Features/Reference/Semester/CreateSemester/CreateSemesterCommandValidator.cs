using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Semester.CreateSemester;

public sealed class CreateSemesterCommandValidator : AbstractValidator<CreateSemesterCommand>
{
    public CreateSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterName)
                    .NotEmpty().WithMessage("Назва семестру не може бути порожньою.")
                    .MinimumLength(2).WithMessage("Назва семестру має містити щонайменше 2 символи.")
                    .MaximumLength(50).WithMessage("Назва семестру не може перевищувати 50 символів.")
                    .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва семестру не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата початку семестру є обов'язковою.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Дата закінчення семестру є обов'язковою.")
            .GreaterThan(x => x.StartDate).WithMessage("Дата закінчення має бути пізнішою за дату початку.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Trim().Length > 0 && name == name.Trim();
    }
}