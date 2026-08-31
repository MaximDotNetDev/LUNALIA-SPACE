using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjects;

public sealed class GetSubjectsQueryHandler(
    ISubjectRepository subjectRepository)
    : IRequestHandler<GetSubjectsQuery, ErrorOr<PagedResponse<SubjectResponse>>>
{
    public async Task<ErrorOr<PagedResponse<SubjectResponse>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await subjectRepository.GetActivePagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            request.SearchTerm,
            cancellationToken).ConfigureAwait(false);

        var responseItems = items.Adapt<IEnumerable<SubjectResponse>>();

        return new PagedResponse<SubjectResponse>(
            responseItems,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}