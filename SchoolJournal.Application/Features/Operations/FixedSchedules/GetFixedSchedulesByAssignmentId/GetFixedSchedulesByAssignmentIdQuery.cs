using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedSchedulesByAssignmentId;

public sealed record GetFixedSchedulesByAssignmentIdQuery(
    Guid AssignmentId
) : IRequest<ErrorOr<IEnumerable<FixedScheduleResponse>>>;