using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Lessons.UpdateLessonTopicAndHomework;

public sealed class UpdateLessonTopicAndHomeworkCommandValidator : AbstractValidator<UpdateLessonTopicAndHomeworkCommand>
{
    public UpdateLessonTopicAndHomeworkCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");

        RuleFor(x => x.LessonTopic)
            .MaximumLength(255).WithMessage("Тема уроку не може перевищувати 255 символів.");

        RuleFor(x => x.Homework)
            .MaximumLength(1000).WithMessage("Домашнє завдання не може перевищувати 1000 символів.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Токен конкурентності (RowVersion) є обов'язковим.")
            .Must(BeAValidBase64String).WithMessage("Токен конкурентності має бути у форматі Base64.");
    }

    private static bool BeAValidBase64String(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
        {
            return false;
        }

        Span<byte> buffer = new Span<byte>(new byte[base64String.Length]);
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}