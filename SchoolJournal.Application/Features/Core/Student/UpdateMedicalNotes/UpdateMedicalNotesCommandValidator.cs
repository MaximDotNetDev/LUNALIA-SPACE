using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.UpdateMedicalNotes;

public sealed class UpdateMedicalNotesCommandValidator : AbstractValidator<UpdateMedicalNotesCommand>
{
    public UpdateMedicalNotesCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.RowVersionBase64).NotEmpty();
        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Медичні нотатки не можуть перевищувати 2000 символів.");
    }
}