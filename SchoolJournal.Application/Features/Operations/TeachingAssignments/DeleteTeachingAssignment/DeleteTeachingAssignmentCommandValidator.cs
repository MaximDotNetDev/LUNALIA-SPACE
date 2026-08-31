using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.DeleteTeachingAssignment;

public sealed class DeleteTeachingAssignmentCommandValidator : AbstractValidator<DeleteTeachingAssignmentCommand>
{
    public DeleteTeachingAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія запису (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Недійсний формат версії запису.");
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        Span<byte> buffer = stackalloc byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}