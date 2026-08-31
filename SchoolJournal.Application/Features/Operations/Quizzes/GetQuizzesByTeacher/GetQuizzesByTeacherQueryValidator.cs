using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesByTeacher;

public sealed class GetQuizzesByTeacherQueryValidator : AbstractValidator<GetQuizzesByTeacherQuery>
{
    public GetQuizzesByTeacherQueryValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");

        RuleFor(x => x.PageRequest.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути більшим або дорівнювати 1.");

        RuleFor(x => x.PageRequest.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Розмір сторінки має бути 1 або більше.")
            .LessThanOrEqualTo(100).WithMessage("Розмір сторінки не може перевищувати 100.");
    }
}