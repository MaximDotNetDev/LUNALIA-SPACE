using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.UpdateStudent;

public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).Must(BeValidName);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).Must(BeValidName);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(x => x.EnrollmentDate)
            .When(x => x.DateOfBirth.HasValue && x.EnrollmentDate.HasValue);
    }

    private static bool BeValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name == name.Trim();
}