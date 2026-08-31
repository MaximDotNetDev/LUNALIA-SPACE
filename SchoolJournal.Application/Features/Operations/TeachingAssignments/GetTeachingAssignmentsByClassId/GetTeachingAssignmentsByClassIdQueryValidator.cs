using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByClassId;

public sealed class GetTeachingAssignmentsByClassIdQueryValidator : AbstractValidator<GetTeachingAssignmentsByClassIdQuery>
{
    public GetTeachingAssignmentsByClassIdQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");

        RuleFor(x => x.Page.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер сторінки повинен бути більшим або дорівнювати 1.");

        RuleFor(x => x.Page.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Розмір сторінки має бути в межах від 1 до 100.");
    }
}