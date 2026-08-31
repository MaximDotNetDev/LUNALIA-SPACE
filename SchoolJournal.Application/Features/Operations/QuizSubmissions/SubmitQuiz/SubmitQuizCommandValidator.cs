using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizSubmissions.SubmitQuiz;

public sealed class SubmitQuizCommandValidator : AbstractValidator<SubmitQuizCommand>
{
    public SubmitQuizCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення не може бути порожнім.");

        RuleFor(x => x.Answers)
            .NotNull().WithMessage("Список відповідей не може бути null.");
    }
}