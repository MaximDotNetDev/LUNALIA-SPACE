using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.RemoveStudentFromSubgroup;

public sealed class RemoveStudentFromSubgroupCommandHandler(
    IStudentSubgroupRepository repository,
    IAuditContext auditContext)
    : IRequestHandler<RemoveStudentFromSubgroupCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RemoveStudentFromSubgroupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existing = await repository.GetByIdAsync(request.StudentSubgroupId, cancellationToken).ConfigureAwait(false);

        if (existing is null || existing.IsDeleted)
        {
            return Error.NotFound(
                code: "StudentSubgroup.NotFound",
                description: "Призначення студента до підгрупи не знайдено або вже видалено.");
        }

        auditContext.TrackOldState(existing);

        var result = await repository.DeleteAsync(request.StudentSubgroupId, cancellationToken).ConfigureAwait(false);

        if (result is not null)
        {
            auditContext.TrackNewState(result);
        }

        return Result.Success;
    }
}