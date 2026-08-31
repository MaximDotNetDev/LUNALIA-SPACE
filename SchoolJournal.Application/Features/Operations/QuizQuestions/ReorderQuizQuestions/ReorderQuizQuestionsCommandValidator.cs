using FluentValidation;
using SchoolJournal.Contracts.DTOs.Operations.QuizQuestions;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.ReorderQuizQuestions;

public sealed class ReorderQuizQuestionsCommandValidator : AbstractValidator<ReorderQuizQuestionsCommand>
{
    public ReorderQuizQuestionsCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Колекція елементів для перевпорядкування не може бути порожньою.")
            .Must(items => HaveUniqueQuestionIds(items)).WithMessage("Ідентифікатори питань мають бути унікальними в межах запиту.")
            .Must(items => HaveUniqueOrderIndices(items)).WithMessage("Порядкові індекси мають бути унікальними в межах запиту.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.QuestionId)
                .NotEmpty().WithMessage("Ідентифікатор питання є обов'язковим.");

            item.RuleFor(i => i.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Порядковий індекс не може бути меншим за 0.");
        });
    }

    private static bool HaveUniqueQuestionIds(IReadOnlyCollection<ReorderItem> items)
    {
        if (items is null || items.Count == 0)
        {
            return false;
        }

        return items.Select(x => x.QuestionId).Distinct().Count() == items.Count;
    }

    private static bool HaveUniqueOrderIndices(IReadOnlyCollection<ReorderItem> items)
    {
        if (items is null || items.Count == 0)
        {
            return false;
        }

        return items.Select(x => x.OrderIndex).Distinct().Count() == items.Count;
    }
}