using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.DeleteTeacherSubstitution;

public sealed class DeleteTeacherSubstitutionCommandValidator : AbstractValidator<DeleteTeacherSubstitutionCommand>
{
    public DeleteTeacherSubstitutionCommandValidator()
    {
        RuleFor(x => x.SubstitutionId)
            .NotEmpty().WithMessage("Ідентифікатор заміни є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Невірний формат RowVersion.");
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return false;
        }

        Span<byte> buffer = new Span<byte>(new byte[base64String.Length]);
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}