using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.AssignTeacherUser;

public sealed class AssignTeacherUserCommandValidator : AbstractValidator<AssignTeacherUserCommand>
{
    public AssignTeacherUserCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних є обов'язковою.");
    }
}