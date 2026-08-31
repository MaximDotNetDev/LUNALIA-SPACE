using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedScheduleById;

public sealed class GetFixedScheduleByIdQueryHandler(IFixedScheduleRepository fixedScheduleRepository)
    : IRequestHandler<GetFixedScheduleByIdQuery, ErrorOr<FixedScheduleResponse>>
{
    public async Task<ErrorOr<FixedScheduleResponse>> Handle(GetFixedScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedule = await fixedScheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken).ConfigureAwait(false);

        if (schedule is null || schedule.IsDeleted)
        {
            return Error.NotFound(
                code: "FixedSchedule.NotFound",
                description: "Запис у розкладі не знайдено або він був видалений.");
        }

        return schedule.Adapt<FixedScheduleResponse>();
    }
}