using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateOddOneOutFromPdf;

public sealed class GenerateOddOneOutFromPdfCommandValidator : AbstractValidator<GenerateOddOneOutFromPdfCommand>
{
    public GenerateOddOneOutFromPdfCommandValidator()
    {
        RuleFor(x => x.PdfBytes)
            .NotEmpty().WithMessage("Файл PDF не може бути порожнім.");

        RuleFor(x => x.QuestionCount)
            .InclusiveBetween(1, 30).WithMessage("Кількість завдань має бути від 1 до 30.");

        RuleFor(x => x.PointsPerQuestion)
            .InclusiveBetween(1, 10).WithMessage("Кількість балів за питання має бути від 1 до 10.");

        RuleFor(x => x.StartPage)
            .GreaterThan(0).When(x => x.StartPage.HasValue).WithMessage("Початкова сторінка має бути більшою за 0.");

        RuleFor(x => x.EndPage)
            .GreaterThanOrEqualTo(x => x.StartPage).When(x => x.StartPage.HasValue && x.EndPage.HasValue)
            .WithMessage("Кінцева сторінка не може бути меншою за початкову.");
    }
}