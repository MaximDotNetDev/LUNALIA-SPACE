using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByRoomId;

public sealed class GetFixedSchedulesByRoomIdQueryHandler(IFixedScheduleRepository fixedScheduleRepository)
    : IRequestHandler<GetFixedSchedulesByRoomIdQuery, ErrorOr<IEnumerable<FixedScheduleResponse>>>
{
    public async Task<ErrorOr<IEnumerable<FixedScheduleResponse>>> Handle(GetFixedSchedulesByRoomIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedules = await fixedScheduleRepository.GetByRoomIdAsync(request.RoomId, cancellationToken).ConfigureAwait(false);

        return schedules.Adapt<IEnumerable<FixedScheduleResponse>>().ToList();
    }
}