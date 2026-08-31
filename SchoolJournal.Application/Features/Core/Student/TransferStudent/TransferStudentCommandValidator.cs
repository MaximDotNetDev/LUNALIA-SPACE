using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.TransferStudent;

public sealed class TransferStudentCommandValidator : AbstractValidator<TransferStudentCommand>
{
    public TransferStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.NewClassId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.");
    }
}