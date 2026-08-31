using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.RestoreLessonType;

public sealed class RestoreLessonTypeCommandHandler(
    ILessonTypeRepository lessonTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreLessonTypeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreLessonTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingType = await lessonTypeRepository.GetByIdAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);

        if (existingType is null)
        {
            return Error.NotFound(
                code: "LessonType.NotFound",
                description: "Тип уроку не знайдено.");
        }

        if (!existingType.IsDeleted)
        {
            return Error.Conflict(
                code: "LessonType.NotDeleted",
                description: "Цей тип уроку активний і не потребує відновлення.");
        }

        auditContext.TrackOldState(existingType);

        var restored = await lessonTypeRepository.RestoreAsync(request.LessonTypeId, cancellationToken).ConfigureAwait(false);

        if (restored is null)
        {
            return Error.Failure(description: "Не вдалося відновити тип уроку.");
        }

        auditContext.TrackNewState(restored);

        return Result.Success;
    }
}