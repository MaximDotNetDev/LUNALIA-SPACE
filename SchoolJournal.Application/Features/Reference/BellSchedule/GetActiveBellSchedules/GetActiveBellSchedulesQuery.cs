using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.GetActiveBellSchedules;

public sealed record GetActiveBellSchedulesQuery(PageRequest PageRequest)
    : IRequest<PagedResponse<BellScheduleResponse>>;