using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва тесту не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва тесту має містити щонайменше 2 символи.")
            .MaximumLength(255).WithMessage("Назва тесту не може перевищувати 255 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва тесту не повинна містити пробіли на початку або в кінці.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        return title.Trim().Length > 0 && title == title.Trim();
    }
}