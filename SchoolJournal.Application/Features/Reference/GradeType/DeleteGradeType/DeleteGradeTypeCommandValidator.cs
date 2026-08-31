using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.GradeType.DeleteGradeType;

public sealed class DeleteGradeTypeCommandValidator : AbstractValidator<DeleteGradeTypeCommand>
{
    public DeleteGradeTypeCommandValidator()
    {
        RuleFor(x => x.GradeTypeId)
            .NotEmpty().WithMessage("Ідентифікатор типу оцінки є обов'язковим.");
    }
}