using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateCrosswordFromPdf;

public sealed class GenerateCrosswordFromPdfCommandValidator : AbstractValidator<GenerateCrosswordFromPdfCommand>
{
    public GenerateCrosswordFromPdfCommandValidator()
    {
        RuleFor(x => x.PdfBytes).NotEmpty().WithMessage("Файл PDF не може бути порожнім.");
        RuleFor(x => x.WordCount).InclusiveBetween(5, 20).WithMessage("Кількість слів для кросворду має бути від 5 до 20.");
        RuleFor(x => x.PointsPerWord).InclusiveBetween(1, 5).WithMessage("Бали за слово мають бути від 1 до 5.");
        RuleFor(x => x.StartPage).GreaterThan(0).When(x => x.StartPage.HasValue).WithMessage("Початкова сторінка має бути більшою за 0.");
        RuleFor(x => x.EndPage).GreaterThanOrEqualTo(x => x.StartPage).When(x => x.StartPage.HasValue && x.EndPage.HasValue).WithMessage("Кінцева сторінка не може бути меншою за початкову.");
    }
}