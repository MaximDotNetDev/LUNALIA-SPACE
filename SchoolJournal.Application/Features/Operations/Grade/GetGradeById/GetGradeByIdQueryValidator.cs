using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradeById;

public sealed class GetGradeByIdQueryValidator : AbstractValidator<GetGradeByIdQuery>
{
    public GetGradeByIdQueryValidator()
    {
        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Ідентифікатор оцінки є обов'язковим.");
    }
}