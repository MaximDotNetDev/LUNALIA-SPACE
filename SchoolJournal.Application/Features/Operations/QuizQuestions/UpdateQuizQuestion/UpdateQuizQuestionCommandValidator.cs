using FluentValidation;
using System.Text.Json;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.UpdateQuizQuestion;

public sealed class UpdateQuizQuestionCommandValidator : AbstractValidator<UpdateQuizQuestionCommand>
{
    public UpdateQuizQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Ідентифікатор питання є обов'язковим.");

        RuleFor(x => x.QuestionText)
            .NotEmpty().WithMessage("Текст питання не може бути порожнім.")
            .MaximumLength(2000).WithMessage("Текст питання не може перевищувати 2000 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Текст питання не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.QuestionType)
            .GreaterThanOrEqualTo(0).WithMessage("Тип питання повинен бути більшим або рівним 0.");

        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(0).WithMessage("Кількість балів не може бути меншою за 0.");

        RuleFor(x => x.ContentJson)
            .NotEmpty().WithMessage("JSON-контент є обов'язковим.")
            .Must(BeValidJson).WithMessage("Контент повинен бути валідним JSON-рядком.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("JSON-контент не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою для забезпечення цілісності даних.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        return text.Trim().Length > 0 && text == text.Trim();
    }

    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }
        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}