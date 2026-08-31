using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentsByQuizId;

public sealed class GetQuizAssignmentsByQuizIdQueryValidator : AbstractValidator<GetQuizAssignmentsByQuizIdQuery>
{
    public GetQuizAssignmentsByQuizIdQueryValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");
    }
}