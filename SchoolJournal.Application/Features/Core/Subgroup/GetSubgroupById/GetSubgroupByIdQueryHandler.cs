using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupById;

public sealed class GetSubgroupByIdQueryHandler(ISubgroupRepository subgroupRepository)
    : IRequestHandler<GetSubgroupByIdQuery, ErrorOr<SubgroupResponse>>
{
    public async Task<ErrorOr<SubgroupResponse>> Handle(GetSubgroupByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subgroup = await subgroupRepository.GetByIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);

        if (subgroup is null)
        {
            return Error.NotFound(
                code: "Subgroup.NotFound",
                description: "Підгрупу не знайдено.");
        }

        return new SubgroupResponse(
                    subgroup.SubgroupId,
                    subgroup.ClassId,
                    string.Empty,
                    subgroup.SubjectId,
                    string.Empty,
                    subgroup.SubgroupName,
                    subgroup.IsActive,
                    Convert.ToBase64String(subgroup.RowVersion.ToArray())
                );
    }
}