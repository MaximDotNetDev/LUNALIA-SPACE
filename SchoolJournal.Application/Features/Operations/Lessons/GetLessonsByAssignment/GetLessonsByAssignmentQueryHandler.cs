using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetLessonsByAssignment;

public sealed class GetLessonsByAssignmentQueryHandler(
    ILessonQueries lessonQueries,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLessonsByAssignmentQuery, ErrorOr<IReadOnlyCollection<LessonResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<LessonResponse>>> Handle(GetLessonsByAssignmentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        var role = currentUserService.GetUserRole();

        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(description: "Користувач не авторизований.");
        }

        if (role != RoleType.Admin && role != RoleType.Director)
        {
            var hasAccess = await lessonQueries.CheckAssignmentReadAccessAsync(
                request.AssignmentId,
                userId,
                role.ToString(),
                cancellationToken).ConfigureAwait(false);

            if (!hasAccess)
            {
                return Error.Forbidden(description: "У вас немає доступу до перегляду уроків цього призначення.");
            }
        }

        var lessons = await lessonQueries.GetByAssignmentIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);
        return lessons.ToList().AsReadOnly();
    }
}