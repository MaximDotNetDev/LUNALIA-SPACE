using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Lessons.CreateLesson;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.LessonDate)
            .NotEmpty().WithMessage("Дата уроку є обов'язковою.");

        RuleFor(x => x.LessonTypeId)
            .NotEmpty().WithMessage("Тип уроку є обов'язковим.");

        RuleFor(x => x.PeriodId)
            .NotEmpty().WithMessage("Ідентифікатор розкладу дзвінків (PeriodId) є обов'язковим.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Ідентифікатор кабінету є обов'язковим.");

        RuleFor(x => x.SemesterId)
            .NotEmpty().WithMessage("Ідентифікатор семестру є обов'язковим.");

        RuleFor(x => x.LessonTopic)
            .MaximumLength(255).WithMessage("Тема уроку не може перевищувати 255 символів.");

        RuleFor(x => x.Homework)
            .MaximumLength(1000).WithMessage("Домашнє завдання не може перевищувати 1000 символів.");
    }
}