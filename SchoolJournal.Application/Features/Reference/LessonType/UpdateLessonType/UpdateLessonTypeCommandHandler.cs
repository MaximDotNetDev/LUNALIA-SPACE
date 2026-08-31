using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.UpdateLessonType;

public sealed class UpdateLessonTypeCommandHandler(
    ILessonTypeRepository lessonTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateLessonTypeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateLessonTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingType = await lessonTypeRepository.GetByIdAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);
        if (existingType is null || existingType.IsDeleted)
        {
            return Error.NotFound(
                code: "LessonType.NotFound",
                description: "Тип уроку не знайдено або він був видалений.");
        }

        if (await lessonTypeRepository.ExistsByNameExcludingIdAsync(request.TypeName, request.LessonTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "LessonType.NameConflict",
                description: $"Тип уроку з назвою '{request.TypeName}' вже існує.");
        }

        auditContext.TrackOldState(existingType);

        var lessonTypeToUpdate = existingType with
        {
            TypeName = request.TypeName,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updated = await lessonTypeRepository.UpdateAsync(lessonTypeToUpdate, cancellationToken).ConfigureAwait(false);
        if (updated is null)
        {
            return Error.Failure(description: "Не вдалося оновити тип уроку.");
        }

        auditContext.TrackNewState(updated);

        return Result.Success;
    }
}