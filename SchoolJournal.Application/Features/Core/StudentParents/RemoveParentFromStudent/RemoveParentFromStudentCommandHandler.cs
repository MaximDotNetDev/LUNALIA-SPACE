using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.RemoveParentFromStudent;

public sealed class RemoveParentFromStudentCommandHandler(
    IStudentParentRepository studentParentRepository,
    IAuditContext auditContext)
    : IRequestHandler<RemoveParentFromStudentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RemoveParentFromStudentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingState = await studentParentRepository.GetByIdAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (existingState is null || existingState.IsDeleted)
        {
            return Error.NotFound(
                code: "StudentParent.NotFound",
                description: $"Зв'язок з ідентифікатором '{request.StudentParentId}' не знайдено або він вже видалений.");
        }

        auditContext.TrackOldState(existingState);

        var deletedEntity = await studentParentRepository.DeleteAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (deletedEntity is not null)
        {
            auditContext.TrackNewState(deletedEntity);
        }

        return Result.Success;
    }
}