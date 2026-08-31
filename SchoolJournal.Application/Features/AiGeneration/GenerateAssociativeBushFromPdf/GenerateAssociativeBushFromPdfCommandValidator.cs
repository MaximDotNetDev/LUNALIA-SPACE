using FluentValidation;

namespace SchoolJournal.Application.Features.AiGeneration.GenerateAssociativeBushFromPdf;

public sealed class GenerateAssociativeBushFromPdfCommandValidator : AbstractValidator<GenerateAssociativeBushFromPdfCommand>
{
    public GenerateAssociativeBushFromPdfCommandValidator()
    {
        RuleFor(x => x.PdfBytes).NotEmpty().WithMessage("Файл PDF не може бути порожнім.");
        RuleFor(x => x.QuestionCount).InclusiveBetween(1, 5).WithMessage("Кількість асоціативних кущів має бути від 1 до 5.");
        RuleFor(x => x.PointsPerQuestion).InclusiveBetween(1, 12).WithMessage("Бали мають бути в діапазоні від 1 до 12.");
        RuleFor(x => x.StartPage).GreaterThan(0).When(x => x.StartPage.HasValue).WithMessage("Початкова сторінка має бути більшою за 0.");
        RuleFor(x => x.EndPage).GreaterThanOrEqualTo(x => x.StartPage).When(x => x.StartPage.HasValue && x.EndPage.HasValue).WithMessage("Кінцева сторінка не може бути меншою за початкову.");
    }
}