using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.GetBellScheduleById;

public sealed record GetBellScheduleByIdQuery(Guid ScheduleId)
    : IRequest<ErrorOr<BellScheduleResponse>>;