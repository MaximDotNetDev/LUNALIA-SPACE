using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Subgroup.UpdateSubgroup;

public sealed class UpdateSubgroupCommandValidator : AbstractValidator<UpdateSubgroupCommand>
{
    public UpdateSubgroupCommandValidator()
    {
        RuleFor(x => x.SubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор підгрупи є обов'язковим.");

        RuleFor(x => x.SubgroupName)
            .NotEmpty().WithMessage("Назва підгрупи не може бути порожньою.")
            .MinimumLength(1).WithMessage("Назва підгрупи не може бути порожньою.")
            .MaximumLength(50).WithMessage("Назва підгрупи не може перевищувати 50 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Назва підгрупи не повинна містити пробіли на початку або в кінці.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("RowVersion є обов'язковим для оновлення.")
            .Must(BeAValidBase64).WithMessage("RowVersion має бути у коректному форматі Base64.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return name.Trim().Length > 0 && name == name.Trim();
    }

    private static bool BeAValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return false;
        }

        Span<byte> buffer = new byte[base64String.Length];
        return Convert.TryFromBase64String(base64String, buffer, out _);
    }
}