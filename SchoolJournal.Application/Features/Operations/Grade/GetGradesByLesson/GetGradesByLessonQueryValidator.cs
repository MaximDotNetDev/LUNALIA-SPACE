using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByLesson;

public sealed class GetGradesByLessonQueryValidator : AbstractValidator<GetGradesByLessonQuery>
{
    public GetGradesByLessonQueryValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");
    }
}