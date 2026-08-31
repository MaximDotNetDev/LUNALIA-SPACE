using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateMultipleChoice;

public sealed class GenerateMultipleChoiceCommandValidator : AbstractValidator<GenerateMultipleChoiceCommand>
{
    public GenerateMultipleChoiceCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст для генерації не може бути порожнім.")
            .MinimumLength(50).WithMessage("Текст занадто короткий. Мінімум 50 символів для якісної генерації.")
            .MaximumLength(100000).WithMessage("Текст занадто великий. Максимум 100 000 символів за один запит.");

        RuleFor(x => x.TotalQuestions)
            .InclusiveBetween(1, 30).WithMessage("Кількість питань має бути від 1 до 30.");

        RuleFor(x => x.MultiAnswerCount)
            .GreaterThanOrEqualTo(0).WithMessage("Не може бути від'ємним.")
            .Must((cmd, multiCount) => multiCount <= cmd.TotalQuestions)
            .WithMessage("Кількість питань з кількома відповідями не може перевищувати загальну кількість питань.");

        RuleFor(x => x.JudgmentCount)
            .GreaterThanOrEqualTo(0).WithMessage("Не може бути від'ємним.")
            .Must((cmd, judmentCount) => judmentCount <= cmd.TotalQuestions)
            .WithMessage("Кількість питань на судження не може перевищувати загальну кількість питань.");

        RuleFor(x => x.PointsPerQuestion)
            .InclusiveBetween(1, 10).WithMessage("Бали за питання мають бути від 1 до 10.");
    }
}