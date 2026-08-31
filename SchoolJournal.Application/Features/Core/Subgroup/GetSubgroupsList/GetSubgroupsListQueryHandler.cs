namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsList;

using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

public sealed class GetSubgroupsListQueryHandler(ISubgroupRepository subgroupRepository)
    : IRequestHandler<GetSubgroupsListQuery, ErrorOr<PagedResponse<SubgroupResponse>>>
{
    public async Task<ErrorOr<PagedResponse<SubgroupResponse>>> Handle(
        GetSubgroupsListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (items, totalCount) = await subgroupRepository.GetPagedAsync(
            request.PageRequest.Skip,
            request.PageRequest.PageSize,
            cancellationToken).ConfigureAwait(false);

        var dtos = items.Select(s => new SubgroupResponse(
                            s.SubgroupId,
                            s.ClassId,
                            s.ClassName,
                            s.SubjectId,
                            s.SubjectName,
                            s.SubgroupName,
                            s.IsActive,
                            Convert.ToBase64String(s.RowVersion.ToArray())
                        ));

        return new PagedResponse<SubgroupResponse>(
            dtos,
            request.PageRequest.PageNumber,
            request.PageRequest.PageSize,
            totalCount);
    }
}