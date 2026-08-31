using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.CreateStudent;

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).Must(BeValidName);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).Must(BeValidName);
        RuleFor(x => x.ClassId).NotEmpty();

        RuleFor(x => x.EnrollmentDate)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .When(x => x.EnrollmentDate.HasValue);

        RuleFor(x => x.DateOfBirth)
            .LessThan(x => x.EnrollmentDate)
            .When(x => x.DateOfBirth.HasValue && x.EnrollmentDate.HasValue)
            .WithMessage("Дата народження повинна бути раніше дати зарахування.");
    }

    private static bool BeValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name == name.Trim();
}