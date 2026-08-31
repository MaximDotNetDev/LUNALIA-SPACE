using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Lessons.DeleteLesson;

public sealed class DeleteLessonCommandHandler(
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<DeleteLessonCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
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
            return Error.Forbidden(description: "Відмовлено у доступі до видалення цього уроку.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await lessonRepository.DeleteAsync(
            request.LessonId,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(description: "Урок вже був видалений або змінений іншим користувачем.");
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