using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.GradeType.DeleteGradeType;

public sealed class DeleteGradeTypeCommandHandler(
    IGradeTypeRepository gradeTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteGradeTypeCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteGradeTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await gradeTypeRepository.GetByIdAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "GradeType.NotFound",
                description: "Тип оцінки не знайдено або його вже було видалено.");
        }

        var deletedEntity = await gradeTypeRepository.DeleteAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        if (deletedEntity is not null)
        {
            auditContext.TrackNewState(deletedEntity);
        }

        return Result.Deleted;
    }
}