using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetScheduleByDateRange;

public sealed record GetScheduleByDateRangeQuery(
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    Guid SemesterId
) : IRequest<ErrorOr<IReadOnlyCollection<LessonResponse>>>;