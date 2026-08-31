using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.DeleteQuizQuestion;

public sealed class DeleteQuizQuestionCommandValidator : AbstractValidator<DeleteQuizQuestionCommand>
{
    public DeleteQuizQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Ідентифікатор питання є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою для перевірки цілісності даних.");
    }
}