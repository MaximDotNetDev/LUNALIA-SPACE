using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.RestoreSubgroup;

public sealed class RestoreSubgroupCommandHandler(
    ISubgroupRepository subgroupRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreSubgroupCommand request, CancellationToken cancellationToken)
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

        if (existingSubgroup is null)
        {
            return Error.NotFound(
                code: "Subgroup.NotFound",
                description: "Підгрупу не знайдено.");
        }

        if (!existingSubgroup.IsDeleted)
        {
            return Error.Conflict(
                code: "Subgroup.NotDeleted",
                description: "Підгрупа не потребує відновлення, оскільки вона не видалена.");
        }

        var oldState = await subgroupRepository.RestoreAsync(request.SubgroupId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Subgroup.Concurrency",
                description: "Не вдалося відновити. Дані були змінені або відновлені іншим користувачем.");
        }

        var newState = await subgroupRepository.GetByIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Success;
    }
}