using MediatR;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;
using Mapster;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypesArchive;

public sealed class GetGradeTypesArchiveQueryHandler(IGradeTypeRepository gradeTypeRepository)
    : IRequestHandler<GetGradeTypesArchiveQuery, PagedResponse<GradeTypeResponse>>
{
    public async Task<PagedResponse<GradeTypeResponse>> Handle(GetGradeTypesArchiveQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await gradeTypeRepository.GetPagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        return new PagedResponse<GradeTypeResponse>(
            items.Adapt<IEnumerable<GradeTypeResponse>>(),
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}