using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.ToggleTeacherStatus;

public sealed class ToggleTeacherStatusCommandValidator : AbstractValidator<ToggleTeacherStatusCommand>
{
    public ToggleTeacherStatusCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних є обов'язковою.");
    }
}