using FluentValidation;

namespace SchoolJournal.Application.Features.Communications.Announcements.ToggleAnnouncementStatus;

public sealed class ToggleAnnouncementStatusCommandValidator : AbstractValidator<ToggleAnnouncementStatusCommand>
{
    public ToggleAnnouncementStatusCommandValidator()
    {
        RuleFor(x => x.AnnouncementId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty().WithMessage("RowVersion обов'язкова.");
    }
}