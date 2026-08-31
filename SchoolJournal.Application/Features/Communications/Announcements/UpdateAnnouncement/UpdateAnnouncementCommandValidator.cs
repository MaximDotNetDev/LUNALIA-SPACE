using FluentValidation;

namespace SchoolJournal.Application.Features.Communications.Announcements.UpdateAnnouncement;

public sealed class UpdateAnnouncementCommandValidator : AbstractValidator<UpdateAnnouncementCommand>
{
    public UpdateAnnouncementCommandValidator()
    {
        RuleFor(x => x.AnnouncementId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Заголовок не може бути порожнім.")
            .MaximumLength(150).WithMessage("Заголовок занадто довгий.");

        RuleFor(x => x.Content).NotEmpty().WithMessage("Вміст не може бути порожнім.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних (RowVersion) обов'язкова для оновлення.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Дата закінчення має бути у майбутньому.")
            .When(x => x.ExpirationDate.HasValue);
    }
}