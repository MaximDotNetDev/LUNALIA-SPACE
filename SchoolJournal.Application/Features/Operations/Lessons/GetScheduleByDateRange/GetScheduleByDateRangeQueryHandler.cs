using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Operations.Lessons.GetLessonById;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetScheduleByDateRange;

public sealed class GetScheduleByDateRangeQueryHandler(
    ILessonQueries lessonQueries,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetScheduleByDateRangeQuery, ErrorOr<IReadOnlyCollection<LessonResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<LessonResponse>>> Handle(GetScheduleByDateRangeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        var role = currentUserService.GetUserRole();

        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(description: "Користувач не авторизований.");
        }

        if (request.StartDate > request.EndDate)
        {
            return Error.Validation(description: "Дата початку не може бути пізнішою за дату завершення.");
        }

        var schedule = await lessonQueries.GetScheduleAsync(
            request.StartDate,
            request.EndDate,
            request.SemesterId,
            userId,
            role.ToString(),
            cancellationToken).ConfigureAwait(false);

        return schedule.ToList().AsReadOnly();
    }
}