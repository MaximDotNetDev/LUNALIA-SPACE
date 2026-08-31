using FluentValidation;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.AssignHomeroomTeacher;

public sealed class AssignHomeroomTeacherCommandValidator : AbstractValidator<AssignHomeroomTeacherCommand>
{
    public AssignHomeroomTeacherCommandValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ID класу обов'язковий.");

        RuleFor(x => x.NewHomeroomTeacherId)
            .NotEmpty().WithMessage("ID нового класного керівника обов'язковий.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) обов'язкова.")
            .Must(BeAValidBase64).WithMessage("Неправильний формат RowVersion.");
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        Span<byte> buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}