using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.GradeType.RestoreGradeType;

public sealed class RestoreGradeTypeCommandValidator : AbstractValidator<RestoreGradeTypeCommand>
{
    public RestoreGradeTypeCommandValidator()
    {
        RuleFor(x => x.GradeTypeId)
            .NotEmpty().WithMessage("Ідентифікатор типу оцінки є обов'язковим.");
    }
}