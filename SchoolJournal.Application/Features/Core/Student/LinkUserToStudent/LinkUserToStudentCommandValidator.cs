using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.LinkUserToStudent;

public sealed class LinkUserToStudentCommandValidator : AbstractValidator<LinkUserToStudentCommand>
{
    public LinkUserToStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty();
    }
}