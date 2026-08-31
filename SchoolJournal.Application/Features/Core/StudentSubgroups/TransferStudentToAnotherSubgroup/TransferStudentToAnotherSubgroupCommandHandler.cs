using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.TransferStudentToAnotherSubgroup;

public sealed class TransferStudentToAnotherSubgroupCommandHandler(
    IStudentSubgroupRepository repository,
    IAuditContext auditContext)
    : IRequestHandler<TransferStudentToAnotherSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(TransferStudentToAnotherSubgroupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await repository.GetByIdAsync(request.StudentSubgroupId, cancellationToken).ConfigureAwait(false);

        if (existing is null || existing.IsDeleted)
        {
            return Error.NotFound(
                code: "StudentSubgroup.NotFound",
                description: "Призначення студента до підгрупи не знайдено або вже видалено.");
        }

        if (existing.SubgroupId == request.NewSubgroupId)
        {
            return Error.Conflict(
                code: "StudentSubgroup.AlreadyInSubgroup",
                description: "Студент вже перебуває у вказаній підгрупі.");
        }

        if (await repository.ExistsActiveAsync(existing.StudentId, request.NewSubgroupId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "StudentSubgroup.AssignmentExists",
                description: "Студент вже має активне призначення до цієї підгрупи.");
        }

        auditContext.TrackOldState(existing);

        var updated = await repository.UpdateSubgroupIdAsync(request.StudentSubgroupId, request.NewSubgroupId, cancellationToken).ConfigureAwait(false);

        if (updated is not null)
        {
            auditContext.TrackNewState(updated);
        }

        return Result.Success;
    }
}