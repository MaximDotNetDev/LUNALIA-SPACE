using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByDay;

public sealed class GetFixedSchedulesByDayQueryHandler(IFixedScheduleRepository fixedScheduleRepository)
    : IRequestHandler<GetFixedSchedulesByDayQuery, ErrorOr<IEnumerable<FixedScheduleResponse>>>
{
    public async Task<ErrorOr<IEnumerable<FixedScheduleResponse>>> Handle(GetFixedSchedulesByDayQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedules = await fixedScheduleRepository.GetByDayAsync(request.DayOfWeek, cancellationToken).ConfigureAwait(false);

        return schedules.Adapt<IEnumerable<FixedScheduleResponse>>().ToList();
    }
}