using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.BellSchedules;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.GetActiveBellSchedules;

public sealed class GetActiveBellSchedulesQueryHandler(IBellScheduleRepository bellScheduleRepository)
    : IRequestHandler<GetActiveBellSchedulesQuery, PagedResponse<BellScheduleResponse>>
{
    public async Task<PagedResponse<BellScheduleResponse>> Handle(GetActiveBellSchedulesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await bellScheduleRepository.GetActivePagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Select(x => new BellScheduleResponse(
            x.ScheduleId,
            x.LessonNumber,
            x.StartTime,
            x.EndTime,
            x.CreatedAt,
            x.UpdatedAt));

        return new PagedResponse<BellScheduleResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}