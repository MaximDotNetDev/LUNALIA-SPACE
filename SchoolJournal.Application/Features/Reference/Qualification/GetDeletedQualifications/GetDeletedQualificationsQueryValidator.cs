using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetDeletedQualifications;

public sealed class GetDeletedQualificationsQueryValidator : AbstractValidator<GetDeletedQualificationsQuery>
{
    public GetDeletedQualificationsQueryValidator()
    {
        RuleFor(x => x.PageRequest.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки має бути більшим або дорівнювати 1.");

        RuleFor(x => x.PageRequest.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Розмір сторінки має бути більшим або дорівнювати 1.")
            .LessThanOrEqualTo(100).WithMessage("Розмір сторінки не може перевищувати 100.");
    }
}