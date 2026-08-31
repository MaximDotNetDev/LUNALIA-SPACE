using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceStats;

public sealed class GetStudentAttendanceStatsQueryValidator : AbstractValidator<GetStudentAttendanceStatsQuery>
{
    public GetStudentAttendanceStatsQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор студента є обов'язковим для формування статистичного звіту.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Кінцева дата аналітичного періоду не може бути меншою за початкову.");
    }
}