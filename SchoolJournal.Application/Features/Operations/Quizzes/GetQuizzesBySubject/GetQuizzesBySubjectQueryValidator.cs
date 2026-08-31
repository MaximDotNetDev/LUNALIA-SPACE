using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesBySubject;

public sealed class GetQuizzesBySubjectQueryValidator : AbstractValidator<GetQuizzesBySubjectQuery>
{
    public GetQuizzesBySubjectQueryValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.PageRequest.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути більшим або дорівнювати 1.");

        RuleFor(x => x.PageRequest.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Розмір сторінки має бути 1 або більше.")
            .LessThanOrEqualTo(100).WithMessage("Розмір сторінки не може перевищувати 100.");
    }
}