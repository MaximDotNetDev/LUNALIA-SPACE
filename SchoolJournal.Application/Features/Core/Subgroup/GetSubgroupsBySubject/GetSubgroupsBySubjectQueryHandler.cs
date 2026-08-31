namespace SchoolJournal.Application.Features.Core.Subgroup.GetSubgroupsBySubject;

using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

public sealed class GetSubgroupsBySubjectQueryHandler(ISubgroupRepository subgroupRepository)
    : IRequestHandler<GetSubgroupsBySubjectQuery, ErrorOr<IReadOnlyCollection<SubgroupResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<SubgroupResponse>>> Handle(
        GetSubgroupsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await subgroupRepository.ClassExistsAsync(request.ClassId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Subgroup.ClassNotFound",
                description: $"Клас з ідентифікатором '{request.ClassId}' не знайдено.");
        }

        if (!await subgroupRepository.SubjectExistsAsync(request.SubjectId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Subgroup.SubjectNotFound",
                description: $"Предмет з ідентифікатором '{request.SubjectId}' не знайдено.");
        }

        var subgroups = await subgroupRepository.GetBySubjectAndClassIdAsync(
            request.ClassId,
            request.SubjectId,
            cancellationToken).ConfigureAwait(false);

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