using FluentValidation;

namespace SchoolJournal.Application.Features.Communications.Announcements.CreateAnnouncement;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок оголошення не може бути порожнім.")
            .MaximumLength(150).WithMessage("Заголовок оголошення не може перевищувати 150 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Заголовок не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Вміст оголошення не може бути порожнім.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Вміст не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Дата закінчення терміну дії повинна бути в майбутньому.")
            .When(x => x.ExpirationDate.HasValue);
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return text.Trim().Length > 0 && text == text.Trim();
    }
}