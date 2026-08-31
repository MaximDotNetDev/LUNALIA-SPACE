using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionById;

public sealed class GetQuizQuestionByIdQueryValidator : AbstractValidator<GetQuizQuestionByIdQuery>
{
    public GetQuizQuestionByIdQueryValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Ідентифікатор питання є обов'язковим.");
    }
}