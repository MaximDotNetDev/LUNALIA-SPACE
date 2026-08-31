using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.GradeType.UpdateGradeType;

public sealed class UpdateGradeTypeCommandHandler(
    IGradeTypeRepository gradeTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateGradeTypeCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateGradeTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await gradeTypeRepository.GetByIdAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "GradeType.NotFound",
                description: "Тип оцінки не знайдено або його було видалено.");
        }

        if (oldState.TypeName == request.TypeName)
        {
            return Result.Updated;
        }

        if (await gradeTypeRepository.ExistsByNameExcludingIdAsync(request.TypeName, request.GradeTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "GradeType.NameConflict",
                description: $"Тип оцінки з назвою '{request.TypeName}' вже існує.");
        }

        var updatedEntity = oldState with
        {
            TypeName = request.TypeName,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var newState = await gradeTypeRepository.UpdateAsync(updatedEntity, cancellationToken).ConfigureAwait(false);

        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return Result.Updated;
    }
}