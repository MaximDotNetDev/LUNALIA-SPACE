using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetActiveQualifications;

public sealed class GetActiveQualificationsQueryValidator : AbstractValidator<GetActiveQualificationsQuery>
{
    public GetActiveQualificationsQueryValidator()
    {
        RuleFor(x => x.PageRequest.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути більшим або дорівнювати 1.");

        RuleFor(x => x.PageRequest.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Розмір сторінки має бути більшим або дорівнювати 1.")
            .LessThanOrEqualTo(100).WithMessage("Розмір сторінки не може перевищувати 100.");
    }
}