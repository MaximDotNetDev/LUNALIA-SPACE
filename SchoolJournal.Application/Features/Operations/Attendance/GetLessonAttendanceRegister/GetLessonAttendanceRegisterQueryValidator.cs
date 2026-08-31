using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetLessonAttendanceRegister;

public sealed class GetLessonAttendanceRegisterQueryValidator : AbstractValidator<GetLessonAttendanceRegisterQuery>
{
    public GetLessonAttendanceRegisterQueryValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("Ідентифікатор уроку є обов'язковим для формування відомості.");
    }
}