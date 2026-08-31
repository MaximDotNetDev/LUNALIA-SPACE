using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subgroup.DeleteSubgroup;

public sealed class DeleteSubgroupCommandHandler(
    ISubgroupRepository subgroupRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteSubgroupCommand request, CancellationToken cancellationToken)
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
                description: "Підгрупу не знайдено або вона вже видалена.");
        }

        var oldState = await subgroupRepository.DeleteAsync(request.SubgroupId, rowVersion, cancellationToken).ConfigureAwait(false);

        if (oldState is null)
        {
            return Error.Conflict(
                code: "Subgroup.Concurrency",
                description: "Не вдалося видалити. Можливо, дані були змінені іншим користувачем.");
        }

        auditContext.TrackNewState(new { IsDeleted = true, DeletedAt = DateTimeOffset.UtcNow });

        return Result.Success;
    }
}