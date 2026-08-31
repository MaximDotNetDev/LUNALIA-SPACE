using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.CreateTeachingAssignment;

public sealed class CreateTeachingAssignmentCommandValidator : AbstractValidator<CreateTeachingAssignmentCommand>
{
    public CreateTeachingAssignmentCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");
    }
}