using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.CreateBellSchedule;

public sealed class CreateBellScheduleCommandHandler(
    IBellScheduleRepository bellScheduleRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateBellScheduleCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateBellScheduleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await bellScheduleRepository.ExistsByLessonNumberAsync(request.LessonNumber, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "BellSchedule.LessonNumberConflict",
                description: $"Розклад для уроку №{request.LessonNumber} вже існує.");
        }

        if (await bellScheduleRepository.HasOverlappingTimeAsync(request.StartTime, request.EndTime, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "BellSchedule.TimeOverlap",
                description: "Вказаний час перетинається з існуючим розкладом іншого уроку.");
        }

        var schedule = new Domain.Entities.Reference.BellSchedule
        {
            LessonNumber = request.LessonNumber,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var scheduleId = await bellScheduleRepository.AddAsync(schedule, cancellationToken).ConfigureAwait(false);

        var newState = await bellScheduleRepository.GetByIdAsync(scheduleId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return scheduleId;
    }
}