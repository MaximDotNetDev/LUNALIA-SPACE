using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subgroup.DeleteSubgroup;

public sealed class DeleteSubgroupCommandValidator : AbstractValidator<DeleteSubgroupCommand>
{
    public DeleteSubgroupCommandValidator()
    {
        RuleFor(x => x.SubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор підгрупи є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("RowVersion є обов'язковим для видалення.")
            .Must(BeAValidBase64).WithMessage("RowVersion має бути у коректному форматі Base64.");
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return false;
        }

        try
        {
            Span<byte> buffer = new byte[base64String.Length];
            return Convert.TryFromBase64String(base64String, buffer, out _);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}