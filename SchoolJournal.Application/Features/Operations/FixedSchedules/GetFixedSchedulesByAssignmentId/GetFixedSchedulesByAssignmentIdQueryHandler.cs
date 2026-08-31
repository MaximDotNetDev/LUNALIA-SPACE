using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;
using SchoolJournal.Domain.Entities.Operations.IRepositories;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByAssignmentId;

public sealed class GetFixedSchedulesByAssignmentIdQueryHandler(IFixedScheduleRepository fixedScheduleRepository)
    : IRequestHandler<GetFixedSchedulesByAssignmentIdQuery, ErrorOr<IEnumerable<FixedScheduleResponse>>>
{
    public async Task<ErrorOr<IEnumerable<FixedScheduleResponse>>> Handle(GetFixedSchedulesByAssignmentIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var schedules = await fixedScheduleRepository.GetByAssignmentIdAsync(request.AssignmentId, cancellationToken).ConfigureAwait(false);

        return schedules.Adapt<IEnumerable<FixedScheduleResponse>>().ToList();
    }
}