using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.SearchStudents;

public sealed class SearchStudentsQueryHandler(IStudentRepository studentRepository)
    : IRequestHandler<SearchStudentsQuery, ErrorOr<PagedResponse<StudentSearchResponse>>>
{
    public async Task<ErrorOr<PagedResponse<StudentSearchResponse>>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await studentRepository.SearchAsync(
            request.SearchTerm,
            request.ClassId,
            request.IsActive,
            request.Page.Skip,
            request.Page.PageSize,
            cancellationToken).ConfigureAwait(false);

        var dtos = items.Adapt<IEnumerable<StudentSearchResponse>>();

        return new PagedResponse<StudentSearchResponse>(
            dtos,
            request.Page.PageNumber,
            request.Page.PageSize,
            totalCount);
    }
}