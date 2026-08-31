using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.UpdateQuizAssignmentDueDate;

public sealed class UpdateQuizAssignmentDueDateCommandValidator : AbstractValidator<UpdateQuizAssignmentDueDateCommand>
{
    public UpdateQuizAssignmentDueDateCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.");

        RuleFor(x => x.DueDate)
            .Must(BeInTheFuture).When(x => x.DueDate.HasValue)
            .WithMessage("Кінцевий термін здачі має бути в майбутньому.");
    }

    private static bool BeInTheFuture(DateTimeOffset? dueDate)
    {
        return dueDate > DateTimeOffset.UtcNow;
    }
}