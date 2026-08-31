using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.RecordAttendance;

public sealed class RecordAttendanceCommandValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор студента є обов'язковим.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Статус відвідуваності не може бути порожнім.")
            .MaximumLength(20).WithMessage("Статус не може перевищувати 20 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Статус не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Comment)
            .MaximumLength(255).WithMessage("Коментар не може перевищувати 255 символів.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return false;
        }

        return status.Trim().Length > 0 && status == status.Trim();
    }
}