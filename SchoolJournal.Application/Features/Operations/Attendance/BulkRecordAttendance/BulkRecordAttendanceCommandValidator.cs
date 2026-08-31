using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.BulkRecordAttendance;

public sealed class BulkRecordAttendanceCommandValidator : AbstractValidator<BulkRecordAttendanceCommand>
{
    public BulkRecordAttendanceCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим.");

        RuleFor(x => x.Students)
            .NotEmpty().WithMessage("Список студентів для виставлення відвідуваності не може бути порожнім.");

        RuleForEach(x => x.Students).ChildRules(student =>
        {
            student.RuleFor(s => s.StudentId)
                .NotEmpty().WithMessage("Ідентифікатор студента є обов'язковим.");

            student.RuleFor(s => s.Status)
                .NotEmpty().WithMessage("Статус відвідуваності не може бути порожнім.")
                .MaximumLength(20).WithMessage("Статус не може перевищувати 20 символів.")
                .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Статус не повинен містити пробіли на початку або в кінці.");

            student.RuleFor(s => s.Comment)
                .MaximumLength(255).WithMessage("Коментар не може перевищувати 255 символів.");
        });
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