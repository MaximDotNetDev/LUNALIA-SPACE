using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Application.Features.Operations.Lessons.GetLessonById;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetClassroomOccupancy;

public sealed class GetClassroomOccupancyQueryHandler(
    ILessonQueries lessonQueries,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetClassroomOccupancyQuery, ErrorOr<IReadOnlyCollection<LessonResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<LessonResponse>>> Handle(GetClassroomOccupancyQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        var role = currentUserService.GetUserRole();

        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(description: "Користувач не авторизований.");
        }

        var occupancy = await lessonQueries.GetRoomOccupancyAsync(
            request.RoomId,
            request.LessonDate,
            request.PeriodId,
            userId,
            role.ToString(),
            cancellationToken).ConfigureAwait(false);

        return occupancy.ToList().AsReadOnly();
    }
}