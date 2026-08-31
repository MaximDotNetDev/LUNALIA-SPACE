using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.DeleteLessonType;

public sealed class DeleteLessonTypeCommandHandler(
    ILessonTypeRepository lessonTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<DeleteLessonTypeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(DeleteLessonTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingType = await lessonTypeRepository.GetByIdAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);

        if (existingType is null || existingType.IsDeleted)
        {
            return Error.NotFound(
                code: "LessonType.NotFound",
                description: "Тип уроку не знайдено або він вже видалений.");
        }

        auditContext.TrackOldState(existingType);

        var deleted = await lessonTypeRepository.DeleteAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);

        if (deleted is null)
        {
            return Error.Failure(description: "Не вдалося видалити тип уроку.");
        }

        auditContext.TrackNewState(deleted);

        return Result.Success;
    }
}