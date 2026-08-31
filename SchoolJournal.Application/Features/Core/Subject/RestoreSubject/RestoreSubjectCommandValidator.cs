using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subject.RestoreSubject;

public sealed class RestoreSubjectCommandValidator : AbstractValidator<RestoreSubjectCommand>
{
    public RestoreSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");
    }
}