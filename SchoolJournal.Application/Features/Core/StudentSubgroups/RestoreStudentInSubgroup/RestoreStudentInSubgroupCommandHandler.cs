using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RestoreStudentInSubgroup;

public sealed class RestoreStudentInSubgroupCommandHandler(
    IStudentSubgroupRepository repository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreStudentInSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreStudentInSubgroupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await repository.GetByIdAsync(request.StudentSubgroupId, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            return Error.NotFound(
                code: "StudentSubgroup.NotFound",
                description: "Призначення студента до підгрупи не знайдено.");
        }

        if (!existing.IsDeleted)
        {
            return Error.Conflict(
                code: "StudentSubgroup.NotDeleted",
                description: "Цей зв'язок вже є активним і не потребує відновлення.");
        }

        if (await repository.ExistsActiveAsync(existing.StudentId, existing.SubgroupId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "StudentSubgroup.ActiveAssignmentExists",
                description: "Неможливо відновити запис: студент вже має інше активне призначення до цієї ж підгрупи.");
        }

        auditContext.TrackOldState(existing);

        var restored = await repository.RestoreAsync(request.StudentSubgroupId, cancellationToken).ConfigureAwait(false);

        if (restored is not null)
        {
            auditContext.TrackNewState(restored);
        }

        return Result.Success;
    }
}