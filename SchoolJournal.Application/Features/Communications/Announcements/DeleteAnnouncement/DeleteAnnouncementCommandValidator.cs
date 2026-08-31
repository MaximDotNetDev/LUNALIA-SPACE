using FluentValidation;

namespace SchoolJournal.Application.Features.Communications.Announcements.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandValidator : AbstractValidator<DeleteAnnouncementCommand>
{
    public DeleteAnnouncementCommandValidator()
    {
        RuleFor(x => x.AnnouncementId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty().WithMessage("RowVersion обов'язкова для видалення.");
    }
}