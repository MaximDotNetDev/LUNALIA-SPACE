using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.CreateQuizAssignment;

public sealed class CreateQuizAssignmentCommandValidator : AbstractValidator<CreateQuizAssignmentCommand>
{
    public CreateQuizAssignmentCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Ідентифікатор тесту є обов'язковим.");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");

        RuleFor(x => x.DueDate)
            .Must(BeInTheFuture).When(x => x.DueDate.HasValue)
            .WithMessage("Кінцевий термін здачі має бути в майбутньому.");
    }

    private static bool BeInTheFuture(DateTimeOffset? dueDate)
    {
        return dueDate > DateTimeOffset.UtcNow;
    }
}