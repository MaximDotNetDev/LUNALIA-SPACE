using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.PedagogicalTitles.DeletePedagogicalTitle;

public sealed class DeletePedagogicalTitleCommandValidator : AbstractValidator<DeletePedagogicalTitleCommand>
{
    public DeletePedagogicalTitleCommandValidator()
    {
        RuleFor(x => x.TitleId)
            .NotEmpty().WithMessage("Ідентифікатор педагогічного звання є обов'язковим.");
    }
}