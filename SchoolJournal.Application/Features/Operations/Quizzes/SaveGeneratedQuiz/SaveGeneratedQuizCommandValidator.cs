using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.SaveGeneratedQuiz;

public sealed class SaveGeneratedQuizCommandValidator : AbstractValidator<SaveGeneratedQuizCommand>
{
    public SaveGeneratedQuizCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва тесту не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва тесту має містити щонайменше 2 символи.")
            .MaximumLength(255).WithMessage("Назва тесту не може перевищувати 255 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва тесту не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Згенерований тест повинен містити хоча б одне питання.");

        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.QuestionText).NotEmpty().WithMessage("Текст питання не може бути порожнім.");
            question.RuleFor(q => q.ContentJson).NotEmpty().WithMessage("JSON контент питання не може бути порожнім.");
            question.RuleFor(q => q.Points).GreaterThan(0).WithMessage("Кількість балів має бути більшою за 0.");
            question.RuleFor(q => q.OrderIndex).GreaterThanOrEqualTo(0).WithMessage("Індекс сортування не може бути від'ємним.");
            question.RuleFor(q => q.QuestionType).GreaterThanOrEqualTo(0).WithMessage("Некоректний тип питання.");
        });
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