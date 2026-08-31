using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByDay;

public sealed record GetFixedSchedulesByDayQuery(
    int DayOfWeek
) : IRequest<ErrorOr<IEnumerable<FixedScheduleResponse>>>;