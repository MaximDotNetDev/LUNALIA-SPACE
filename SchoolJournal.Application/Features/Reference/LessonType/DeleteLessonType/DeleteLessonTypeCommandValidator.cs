using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.LessonType.DeleteLessonType;

public sealed class DeleteLessonTypeCommandValidator : AbstractValidator<DeleteLessonTypeCommand>
{
    public DeleteLessonTypeCommandValidator()
    {
        RuleFor(x => x.LessonTypeId)
            .NotEmpty().WithMessage("ID типу уроку є обов'язковим.");
    }
}