using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.TeacherSubstitution.UpdateTeacherSubstitution;

public sealed class UpdateTeacherSubstitutionCommandValidator : AbstractValidator<UpdateTeacherSubstitutionCommand>
{
    public UpdateTeacherSubstitutionCommandValidator()
    {
        RuleFor(x => x.SubstitutionId)
            .NotEmpty().WithMessage("Ідентифікатор заміни є обов'язковим.");

        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("Ідентифікатор призначення є обов'язковим.");

        RuleFor(x => x.SubstituteTeacherId)
            .NotEmpty().WithMessage("Ідентифікатор вчителя на заміну є обов'язковим.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Дата початку заміни є обов'язковою.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Дата закінчення заміни є обов'язковою.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Дата закінчення має бути більшою або дорівнювати даті початку.");

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