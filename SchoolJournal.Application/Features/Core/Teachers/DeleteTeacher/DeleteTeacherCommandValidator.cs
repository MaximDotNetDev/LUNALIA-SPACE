using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.DeleteTeacher;

public sealed class DeleteTeacherCommandValidator : AbstractValidator<DeleteTeacherCommand>
{
    public DeleteTeacherCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних є обов'язковою для видалення.");
    }
}