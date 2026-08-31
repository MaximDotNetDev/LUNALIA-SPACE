using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.DeleteStudent;

public sealed class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentCommand>
{
    public DeleteStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty().WithMessage("Версія запису обов'язкова.");
    }
}