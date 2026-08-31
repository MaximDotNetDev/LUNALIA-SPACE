using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subject.UpdateSubject;

public sealed class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.SubjectName)
            .NotEmpty().WithMessage("Назва предмета не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва предмета має містити щонайменше 2 символи.")
            .MaximumLength(100).WithMessage("Назва предмета не може перевищувати 100 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва предмета не повинна містити пробіли на початку або в кінці.");
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