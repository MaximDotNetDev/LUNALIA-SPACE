using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.Lessons.CreateLesson;

public sealed class CreateLessonCommandHandler(
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<CreateLessonCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserService.GetUserId();

        if (userId == Guid.Empty)
        {
            return Error.Unauthorized(
                code: "Lesson.Unauthorized",
                description: "Користувач не ідентифікований.");
        }

        var isOwner = await lessonRepository.VerifyAssignmentOwnershipAsync(request.AssignmentId, userId, cancellationToken).ConfigureAwait(false);
        if (!isOwner)
        {
            return Error.Forbidden(
                code: "Lesson.Forbidden",
                description: "Ви не маєте доступу до цього призначення або воно не існує.");
        }

        var lesson = new Domain.Entities.Operations.Lesson
        {
            AssignmentId = request.AssignmentId,
            LessonDate = request.LessonDate,
            LessonTopic = request.LessonTopic,
            Homework = request.Homework,
            LessonTypeId = request.LessonTypeId,
            PeriodId = request.PeriodId,
            RoomId = request.RoomId,
            SemesterId = request.SemesterId
        };

        var lessonId = await lessonRepository.AddAsync(lesson, cancellationToken).ConfigureAwait(false);

        var newState = await lessonRepository.GetByIdAsync(lessonId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return lessonId;
    }
}