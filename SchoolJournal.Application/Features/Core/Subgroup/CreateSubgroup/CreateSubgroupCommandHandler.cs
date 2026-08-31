using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.CreateSubgroup;

public sealed class CreateSubgroupCommandHandler(
    ISubgroupRepository subgroupRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateSubgroupCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateSubgroupCommand request, CancellationToken cancellationToken)
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

        if (await subgroupRepository.ExistsByNameInClassAsync(request.ClassId, request.SubgroupName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Subgroup.NameConflict",
                description: $"Підгрупа з назвою '{request.SubgroupName}' вже існує в даному класі.");
        }

        var subgroup = new Domain.Entities.Core.Subgroup
        {
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            SubgroupName = request.SubgroupName
        };

        var subgroupId = await subgroupRepository.AddAsync(subgroup, cancellationToken).ConfigureAwait(false);

        var newState = await subgroupRepository.GetByIdAsync(subgroupId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return subgroupId;
    }
}