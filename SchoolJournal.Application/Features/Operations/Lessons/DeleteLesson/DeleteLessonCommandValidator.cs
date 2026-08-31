using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Lessons.DeleteLesson;

public sealed class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Токен конкурентності є обов'язковим.")
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