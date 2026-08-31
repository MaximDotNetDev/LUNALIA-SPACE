using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByDay;

public sealed class GetFixedSchedulesByDayQueryValidator : AbstractValidator<GetFixedSchedulesByDayQuery>
{
    public GetFixedSchedulesByDayQueryValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(1, 7)
            .WithMessage("День тижня має бути в межах від понеділка до неділі (1-7).");
    }
}