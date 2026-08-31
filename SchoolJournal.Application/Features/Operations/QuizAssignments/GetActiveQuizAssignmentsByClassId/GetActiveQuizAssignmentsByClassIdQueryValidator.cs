using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetActiveQuizAssignmentsByClassId;

public sealed class GetActiveQuizAssignmentsByClassIdQueryValidator : AbstractValidator<GetActiveQuizAssignmentsByClassIdQuery>
{
    public GetActiveQuizAssignmentsByClassIdQueryValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");
    }
}