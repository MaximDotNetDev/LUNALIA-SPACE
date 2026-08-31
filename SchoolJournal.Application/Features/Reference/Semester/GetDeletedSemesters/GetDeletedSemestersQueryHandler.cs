using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Semester.GetDeletedSemesters;
public sealed class GetDeletedSemestersQueryHandler(ISemesterRepository semesterRepository)
    : IRequestHandler<GetDeletedSemestersQuery, PagedResponse<SemesterResponse>>
{
    public async Task<PagedResponse<SemesterResponse>> Handle(GetDeletedSemestersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await semesterRepository.GetDeletedPagedAsync(
            request.Pagination.Skip,
            request.Pagination.PageSize,
            cancellationToken).ConfigureAwait(false);

        var responses = items.Select(s => new SemesterResponse(
            s.SemesterId,
            s.SemesterName,
            s.StartDate,
            s.EndDate,
            Convert.ToBase64String(s.RowVersion.ToArray())));

        return new PagedResponse<SemesterResponse>(
            responses,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            totalCount);
    }
}