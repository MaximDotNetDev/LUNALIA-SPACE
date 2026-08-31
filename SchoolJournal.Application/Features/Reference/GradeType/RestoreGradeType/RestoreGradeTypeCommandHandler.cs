using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.GradeType.RestoreGradeType;

public sealed class RestoreGradeTypeCommandHandler(
    IGradeTypeRepository gradeTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<RestoreGradeTypeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RestoreGradeTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var oldState = await gradeTypeRepository.GetByIdAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        if (oldState is null || !oldState.IsDeleted)
        {
            return Error.NotFound(
                code: "GradeType.NotFound",
                description: "Тип оцінки не знайдено або він не є видаленим.");
        }

        if (await gradeTypeRepository.ExistsByNameExcludingIdAsync(oldState.TypeName, request.GradeTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "GradeType.RestoreConflict",
                description: $"Неможливо відновити. Активний тип оцінки з назвою '{oldState.TypeName}' вже існує.");
        }

        var restoredEntity = await gradeTypeRepository.RestoreAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        if (restoredEntity is not null)
        {
            auditContext.TrackNewState(restoredEntity);
        }

        return Result.Success;
    }
}