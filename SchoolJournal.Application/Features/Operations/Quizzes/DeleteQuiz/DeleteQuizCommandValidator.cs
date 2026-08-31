using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.DeleteQuiz;

public sealed class DeleteQuizCommandValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("RowVersion є обов'язковим для видалення.")
            .Must(BeAValidBase64).WithMessage("RowVersion має бути валідним Base64 рядком.");
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return false;
        }

        Span<byte> buffer = new byte[base64String.Length];
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}