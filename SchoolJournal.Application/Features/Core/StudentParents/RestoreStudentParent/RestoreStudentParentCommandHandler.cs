using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentParents.RestoreStudentParent;

public sealed class RestoreStudentParentCommandHandler(
    IStudentParentRepository studentParentRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreStudentParentCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreStudentParentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingState = await studentParentRepository.GetByIdAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (existingState is null)
        {
            return Error.NotFound(
                code: "StudentParent.NotFound",
                description: $"Зв'язок з ідентифікатором '{request.StudentParentId}' не знайдено.");
        }

        if (!existingState.IsDeleted)
        {
            return Error.Conflict(
                code: "StudentParent.NotDeleted",
                description: "Цей зв'язок не є видаленим, тому його неможливо відновити.");
        }

        auditContext.TrackOldState(existingState);

        var restoredEntity = await studentParentRepository.RestoreAsync(request.StudentParentId, cancellationToken).ConfigureAwait(false);

        if (restoredEntity is not null)
        {
            auditContext.TrackNewState(restoredEntity);
        }

        return Result.Success;
    }
}