using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentsBySubjectId;

public sealed class GetTeachingAssignmentsBySubjectIdQueryHandler(
    ITeachingAssignmentQueries teachingAssignmentQueries)
    : IRequestHandler<GetTeachingAssignmentsBySubjectIdQuery, PagedResponse<TeachingAssignmentResponse>>
{
    public async Task<PagedResponse<TeachingAssignmentResponse>> Handle(GetTeachingAssignmentsBySubjectIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await teachingAssignmentQueries.GetPagedBySubjectIdAsync(
            request.SubjectId,
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