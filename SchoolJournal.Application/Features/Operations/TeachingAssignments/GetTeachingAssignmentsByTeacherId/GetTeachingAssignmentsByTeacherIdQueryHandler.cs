using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsByTeacherId;

public sealed class GetTeachingAssignmentsByTeacherIdQueryHandler(
    ITeachingAssignmentQueries teachingAssignmentQueries)
    : IRequestHandler<GetTeachingAssignmentsByTeacherIdQuery, PagedResponse<TeachingAssignmentResponse>>
{
    public async Task<PagedResponse<TeachingAssignmentResponse>> Handle(GetTeachingAssignmentsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await teachingAssignmentQueries.GetPagedByTeacherIdAsync(
            request.TeacherId,
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