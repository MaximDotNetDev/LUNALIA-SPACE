using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Lessons.RescheduleLesson;

public sealed class RescheduleLessonCommandHandler(
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<RescheduleLessonCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RescheduleLessonCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(description: "Користувач не ідентифікований.");
        }

        var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(request.LessonId, userId, cancellationToken).ConfigureAwait(false);
        if (!isOwner)
        {
            return Error.Forbidden(description: "Відмовлено у доступі до перенесення цього уроку.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await lessonRepository.RescheduleAsync(
            request.LessonId,
            request.LessonDate,
            request.PeriodId,
            request.RoomId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(description: "Конфлікт паралельного доступу. Дані уроку були змінені.");
        }

        auditContext.TrackOldState(oldState);

        var newState = await lessonRepository.GetByIdAsync(request.LessonId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}