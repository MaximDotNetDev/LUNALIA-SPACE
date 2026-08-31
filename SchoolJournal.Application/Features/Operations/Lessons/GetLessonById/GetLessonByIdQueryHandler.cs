using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetLessonById;

public sealed class GetLessonByIdQueryHandler(
    ILessonQueries lessonQueries,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLessonByIdQuery, ErrorOr<LessonResponse>>
{
    public async Task<ErrorOr<LessonResponse>> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        var role = currentUserService.GetUserRole();

        if (userId == Guid.Empty) return Error.Unauthorized();

        if (role != RoleType.Admin && role != RoleType.Director)
        {
            var hasAccess = await lessonQueries.CheckReadAccessAsync(
                request.LessonId,
                userId,
                role.ToString(),
                cancellationToken).ConfigureAwait(false);

            if (!hasAccess)
            {
                return Error.Forbidden(description: "У вас немає доступу до перегляду цього уроку.");
            }
        }

        var lesson = await lessonQueries.GetDetailedByIdAsync(request.LessonId, cancellationToken).ConfigureAwait(false);

        return lesson is null
            ? Error.NotFound(description: "Урок не знайдено.")
            : lesson;
    }
}