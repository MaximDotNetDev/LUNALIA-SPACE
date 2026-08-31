using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizQuestions.GetQuizQuestionsByQuizId;

public sealed class GetQuizQuestionsByQuizIdQueryValidator : AbstractValidator<GetQuizQuestionsByQuizIdQuery>
{
    public GetQuizQuestionsByQuizIdQueryValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");

        RuleFor(x => x.PageRequest)
            .NotNull().WithMessage("Параметри пагінації є обов'язковими.");

        When(x => x.PageRequest is not null, () =>
        {
            RuleFor(x => x.PageRequest.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки повинен бути більшим або рівним 1.");

            RuleFor(x => x.PageRequest.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Розмір сторінки повинен бути від 1 до 100.");
        });
    }
}