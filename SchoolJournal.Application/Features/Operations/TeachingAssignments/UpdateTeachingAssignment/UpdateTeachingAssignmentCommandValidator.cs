using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.UpdateTeachingAssignment;

public sealed class UpdateTeachingAssignmentCommandValidator : AbstractValidator<UpdateTeachingAssignmentCommand>
{
    public UpdateTeachingAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя є обов'язковим.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Ідентифікатор предмета є обов'язковим.");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Ідентифікатор класу є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Недійсний формат версії запису.");
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return false;
        }

        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}