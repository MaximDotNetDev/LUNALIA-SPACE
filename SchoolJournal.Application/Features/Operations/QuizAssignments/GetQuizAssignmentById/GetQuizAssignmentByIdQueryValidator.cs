using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentById;

public sealed class GetQuizAssignmentByIdQueryValidator : AbstractValidator<GetQuizAssignmentByIdQuery>
{
    public GetQuizAssignmentByIdQueryValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим для запиту.");
    }
}