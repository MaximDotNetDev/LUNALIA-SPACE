using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Position.DeletePosition;

public sealed class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
{
    public DeletePositionCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Ідентифікатор посади не може бути порожнім.");
    }
}