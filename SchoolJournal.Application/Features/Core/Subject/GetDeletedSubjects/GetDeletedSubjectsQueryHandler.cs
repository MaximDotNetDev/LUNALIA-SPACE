using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.GetDeletedSubjects;

public sealed class GetDeletedSubjectsQueryHandler(
    ISubjectRepository subjectRepository)
    : IRequestHandler<GetDeletedSubjectsQuery, ErrorOr<PagedResponse<SubjectResponse>>>
{
    public async Task<ErrorOr<PagedResponse<SubjectResponse>>> Handle(GetDeletedSubjectsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await subjectRepository.GetDeletedPagedAsync(
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