using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.GetBellScheduleById;

public sealed class GetBellScheduleByIdQueryHandler(IBellScheduleRepository bellScheduleRepository)
    : IRequestHandler<GetBellScheduleByIdQuery, ErrorOr<BellScheduleResponse>>
{
    public async Task<ErrorOr<BellScheduleResponse>> Handle(GetBellScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedule = await bellScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);

        if (schedule is null || schedule.IsDeleted)
        {
            return Error.NotFound(
                code: "BellSchedule.NotFound",
                description: "Розклад дзвінків не знайдено.");
        }

        return new BellScheduleResponse(
            schedule.ScheduleId,
            schedule.LessonNumber,
            schedule.StartTime,
            schedule.EndTime,
            schedule.CreatedAt,
            schedule.UpdatedAt);
    }
}