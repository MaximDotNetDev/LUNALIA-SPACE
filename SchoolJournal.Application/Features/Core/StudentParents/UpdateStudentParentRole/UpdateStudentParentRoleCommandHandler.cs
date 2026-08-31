using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.UpdateStudentParentRole;

public sealed class UpdateStudentParentRoleCommandHandler(
    IStudentParentRepository studentParentRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateStudentParentRoleCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateStudentParentRoleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingState = await studentParentRepository.GetByIdAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (existingState is null || existingState.IsDeleted)
        {
            return Error.NotFound(
                code: "StudentParent.NotFound",
                description: $"Зв'язок з ідентифікатором '{request.StudentParentId}' не знайдено або він був видалений.");
        }

        auditContext.TrackOldState(existingState);

        var updatedEntity = await studentParentRepository.UpdateRoleAsync(request.StudentParentId, request.Role, cancellationToken).ConfigureAwait(false);

        if (updatedEntity is not null)
        {
            auditContext.TrackNewState(updatedEntity);
        }

        return Result.Success;
    }
}