namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.RestorePedagogicalTitle;

using FluentValidation;

public sealed class RestorePedagogicalTitleCommandValidator : AbstractValidator<RestorePedagogicalTitleCommand>
{
    public RestorePedagogicalTitleCommandValidator()
    {
        RuleFor(x => x.TitleId)
            .NotEmpty().WithMessage("Ідентифікатор педагогічного звання є обов'язковим.");
    }
}