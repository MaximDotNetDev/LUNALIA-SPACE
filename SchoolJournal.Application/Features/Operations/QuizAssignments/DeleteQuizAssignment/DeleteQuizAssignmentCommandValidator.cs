using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.DeleteQuizAssignment;

public sealed class DeleteQuizAssignmentCommandValidator : AbstractValidator<DeleteQuizAssignmentCommand>
{
    public DeleteQuizAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою для видалення.");
    }
}