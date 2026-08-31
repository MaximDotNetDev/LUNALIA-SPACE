using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateMatchingFromPdf;

public sealed class GenerateMatchingFromPdfCommandValidator : AbstractValidator<GenerateMatchingFromPdfCommand>
{
    public GenerateMatchingFromPdfCommandValidator()
    {
        RuleFor(x => x.PdfBytes).NotEmpty().WithMessage("Файл PDF не може бути порожнім.");
        RuleFor(x => x.QuestionCount).InclusiveBetween(1, 20).WithMessage("Кількість завдань на відповідність має бути від 1 до 20.");
        RuleFor(x => x.PointsPerQuestion).InclusiveBetween(1, 10).WithMessage("Бали мають бути в діапазоні від 1 до 10.");
        RuleFor(x => x.StartPage).GreaterThan(0).When(x => x.StartPage.HasValue).WithMessage("Початкова сторінка має бути більшою за 0.");
        RuleFor(x => x.EndPage).GreaterThanOrEqualTo(x => x.StartPage).When(x => x.StartPage.HasValue && x.EndPage.HasValue).WithMessage("Кінцева сторінка не може бути меншою за початкову.");
    }
}