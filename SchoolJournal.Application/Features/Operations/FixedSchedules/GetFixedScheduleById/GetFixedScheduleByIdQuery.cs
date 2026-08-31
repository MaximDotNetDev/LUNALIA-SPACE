using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.GetFixedScheduleById;

public sealed record GetFixedScheduleByIdQuery(
    Guid ScheduleId
) : IRequest<ErrorOr<FixedScheduleResponse>>;