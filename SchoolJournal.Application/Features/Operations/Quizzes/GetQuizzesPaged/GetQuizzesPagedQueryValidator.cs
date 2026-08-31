using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesPaged;

public sealed class GetQuizzesPagedQueryValidator : AbstractValidator<GetQuizzesPagedQuery>
{
    public GetQuizzesPagedQueryValidator()
    {
        RuleFor(x => x.PageRequest.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути більшим або дорівнювати 1.");

        RuleFor(x => x.PageRequest.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Розмір сторінки має бути 1 або більше.")
            .LessThanOrEqualTo(100).WithMessage("Розмір сторінки не може перевищувати 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Довжина пошукового запиту не може перевищувати 100 символів.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}