using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subject.DeleteSubject;

public sealed class DeleteSubjectCommandValidator : AbstractValidator<DeleteSubjectCommand>
{
    public DeleteSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");
    }
}