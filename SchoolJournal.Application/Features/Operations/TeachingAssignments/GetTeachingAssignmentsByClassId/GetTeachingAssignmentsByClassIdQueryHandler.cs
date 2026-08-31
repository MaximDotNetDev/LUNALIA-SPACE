using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByClassId;

public sealed class GetTeachingAssignmentsByClassIdQueryHandler(
    ITeachingAssignmentQueries teachingAssignmentQueries)
    : IRequestHandler<GetTeachingAssignmentsByClassIdQuery, PagedResponse<TeachingAssignmentResponse>>
{
    public async Task<PagedResponse<TeachingAssignmentResponse>> Handle(GetTeachingAssignmentsByClassIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await teachingAssignmentQueries.GetPagedByClassIdAsync(
            request.ClassId,
            request.Page.Skip,
            request.Page.PageSize,
            cancellationToken).ConfigureAwait(false);

        return new PagedResponse<TeachingAssignmentResponse>(
            items,
            request.Page.PageNumber,
            request.Page.PageSize,
            totalCount);
    }
}