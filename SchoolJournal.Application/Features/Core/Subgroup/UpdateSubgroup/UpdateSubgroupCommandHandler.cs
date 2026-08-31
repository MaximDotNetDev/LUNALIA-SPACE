using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.UpdateSubgroup;

public sealed class UpdateSubgroupCommandHandler(
    ISubgroupRepository subgroupRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateSubgroupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        }
        catch (FormatException)
        {
            return Error.Validation(code: "Subgroup.InvalidRowVersion", description: "Некоректний формат RowVersion.");
        }

        var existingSubgroup = await subgroupRepository.GetByIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);

        if (existingSubgroup is null || existingSubgroup.IsDeleted)
        {
            return Error.NotFound(
                code: "Subgroup.NotFound",
                description: "Підгрупу не знайдено або вона була видалена.");
        }

        if (await subgroupRepository.ExistsByNameInClassExcludingIdAsync(existingSubgroup.ClassId, request.SubgroupName, request.SubgroupId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Subgroup.NameConflict",
                description: $"Інша підгрупа з назвою '{request.SubgroupName}' вже існує в цьому класі.");
        }

        var subgroupToUpdate = existingSubgroup with
        {
            SubgroupName = request.SubgroupName,
            IsActive = request.IsActive,
            RowVersion = rowVersion,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var oldState = await subgroupRepository.UpdateAsync(subgroupToUpdate, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Subgroup.Concurrency",
                description: "Дані були змінені іншим користувачем. Будь ласка, оновіть сторінку та спробуйте ще раз.");
        }

        var newState = await subgroupRepository.GetByIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}