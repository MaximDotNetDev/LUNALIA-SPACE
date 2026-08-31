using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.LessonType.RestoreLessonType;

public sealed class RestoreLessonTypeCommandValidator : AbstractValidator<RestoreLessonTypeCommand>
{
    public RestoreLessonTypeCommandValidator()
    {
        RuleFor(x => x.LessonTypeId)
            .NotEmpty().WithMessage("ID типу уроку є обов'язковим.");
    }
}