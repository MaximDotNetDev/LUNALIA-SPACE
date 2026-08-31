using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Lessons.UpdateLessonTopicAndHomework;

public sealed class UpdateLessonTopicAndHomeworkCommandHandler(
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateLessonTopicAndHomeworkCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateLessonTopicAndHomeworkCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(
                code: "Lesson.Unauthorized",
                description: "Користувач не ідентифікований.");
        }

        var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(request.LessonId, userId, cancellationToken).ConfigureAwait(false);
        if (!isOwner)
        {
            return Error.Forbidden(
                code: "Lesson.Forbidden",
                description: "Ви не маєте доступу до редагування цього уроку або він не існує.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var oldState = await lessonRepository.UpdateTopicAndHomeworkAsync(
            request.LessonId,
            request.LessonTopic,
            request.Homework,
            rowVersionBytes,
            cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Lesson.ConcurrencyViolation",
                description: "Урок був змінений іншим користувачем або видалений. Будь ласка, оновіть сторінку та спробуйте ще раз.");
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