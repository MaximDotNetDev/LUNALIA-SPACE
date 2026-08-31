using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsByClass;

public sealed class GetSubgroupsByClassQueryHandler(ISubgroupRepository subgroupRepository)
    : IRequestHandler<GetSubgroupsByClassQuery, ErrorOr<IReadOnlyCollection<SubgroupResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<SubgroupResponse>>> Handle(
        GetSubgroupsByClassQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await subgroupRepository.ClassExistsAsync(request.ClassId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Subgroup.ClassNotFound",
                description: $"Клас з ідентифікатором '{request.ClassId}' не знайдено.");
        }

        var subgroups = await subgroupRepository.GetByClassIdAsync(request.ClassId, cancellationToken).ConfigureAwait(false);

        var response = subgroups.Select(s => new SubgroupResponse(
                    s.SubgroupId,
                    s.ClassId,
                    string.Empty,
                    s.SubjectId,
                    string.Empty,
                    s.SubgroupName,
                    s.IsActive,
                    Convert.ToBase64String(s.RowVersion.ToArray())
                )).ToList().AsReadOnly();

        return response;
    }
}