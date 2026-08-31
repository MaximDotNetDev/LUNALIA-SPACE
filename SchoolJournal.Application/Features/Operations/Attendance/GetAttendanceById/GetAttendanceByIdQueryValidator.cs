using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetAttendanceById;

public sealed class GetAttendanceByIdQueryValidator : AbstractValidator<GetAttendanceByIdQuery>
{
    public GetAttendanceByIdQueryValidator()
    {
        RuleFor(x => x.AttendanceId)
            .NotEmpty().WithMessage("Ідентифікатор запису відвідуваності є обов'язковим для запиту.");
    }
}