using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateFromPdf;

public sealed class GenerateMultipleChoiceFromPdfCommandValidator : AbstractValidator<GenerateMultipleChoiceFromPdfCommand>
{
    public GenerateMultipleChoiceFromPdfCommandValidator()
    {
        RuleFor(x => x.PdfBytes)
            .NotEmpty().WithMessage("Файл PDF не може бути порожнім.");

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

        RuleFor(x => x.StartPage)
            .GreaterThan(0).When(x => x.StartPage.HasValue).WithMessage("Початкова сторінка має бути більшою за 0.");

        RuleFor(x => x.EndPage)
            .GreaterThanOrEqualTo(x => x.StartPage).When(x => x.StartPage.HasValue && x.EndPage.HasValue)
            .WithMessage("Кінцева сторінка не може бути меншою за початкову.");
    }
}