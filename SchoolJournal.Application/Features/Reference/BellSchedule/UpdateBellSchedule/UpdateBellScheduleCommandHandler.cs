using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.UpdateBellSchedule;

public sealed class UpdateBellScheduleCommandHandler(
    IBellScheduleRepository bellScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateBellScheduleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateBellScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await bellScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);
        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "BellSchedule.NotFound",
                description: "Розклад дзвінків не знайдено або він був видалений.");
        }

        if (await bellScheduleRepository.ExistsByLessonNumberExcludingIdAsync(request.LessonNumber, request.ScheduleId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "BellSchedule.LessonNumberConflict",
                description: $"Розклад для уроку №{request.LessonNumber} вже існує.");
        }

        if (await bellScheduleRepository.HasOverlappingTimeExcludingIdAsync(request.StartTime, request.EndTime, request.ScheduleId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "BellSchedule.TimeOverlap",
                description: "Вказаний час перетинається з іншим розкладом.");
        }

        var trackOldStateMethod = auditContext.GetType().GetMethod("TrackOldState");
        trackOldStateMethod?.Invoke(auditContext, [oldState]);

        var updateModel = new Domain.Entities.Reference.BellSchedule
        {
            ScheduleId = request.ScheduleId,
            LessonNumber = request.LessonNumber,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var newState = await bellScheduleRepository.UpdateAsync(updateModel, cancellationToken).ConfigureAwait(false);

        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}