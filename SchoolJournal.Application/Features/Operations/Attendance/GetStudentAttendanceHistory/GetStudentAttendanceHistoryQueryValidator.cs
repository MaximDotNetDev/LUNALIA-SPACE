using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryQueryValidator : AbstractValidator<GetStudentAttendanceHistoryQuery>
{
    public GetStudentAttendanceHistoryQueryValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Ідентифікатор студента є обов'язковим для формування історії.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Кінцева дата періоду не може бути меншою за початкову дату.");
    }
}