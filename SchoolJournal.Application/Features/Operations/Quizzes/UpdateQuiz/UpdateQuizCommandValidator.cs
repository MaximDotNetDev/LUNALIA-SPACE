using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва тесту не може бути порожньою.")
            .MinimumLength(2).WithMessage("Назва тесту має містити щонайменше 2 символи.")
            .MaximumLength(255).WithMessage("Назва тесту не може перевищувати 255 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва тесту не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.RowVersionBase64)
                    .NotEmpty().WithMessage("RowVersion є обов'язковим для оновлення.")
                    .Must(BeAValidBase64).WithMessage("RowVersion має бути валідним Base64 рядком.");

        RuleFor(x => x.Questions)
            .NotNull().WithMessage("Колекція питань не може бути NULL.");

        RuleForEach(x => x.Questions).ChildRules(q =>
        {
            q.RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Індекс черговості питання не може бути від'ємним.");

            q.RuleFor(x => x.QuestionText)
                .NotEmpty().WithMessage("Текст питання не може бути порожнім.")
                .MaximumLength(2000).WithMessage("Текст питання не може перевищувати 2000 символів.");

            q.RuleFor(x => x.QuestionType)
                .GreaterThanOrEqualTo(0).WithMessage("Тип питання має бути коректним числовим значенням.");

            q.RuleFor(x => x.ContentJson)
                .NotEmpty().WithMessage("JSON-контент питання є обов'язковим.")
                .Must(IsValidJson).WithMessage("Поле ContentJson повинно містити валідний рядок JSON.");

            q.RuleFor(x => x.Points)
                .GreaterThanOrEqualTo(0).WithMessage("Кількість балів за питання не може бути від'ємною.");
        });
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        return title.Trim().Length > 0 && title == title.Trim();
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