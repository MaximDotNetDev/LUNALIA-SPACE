using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.UpdateSchoolClass;

public sealed class UpdateSchoolClassCommandHandler(
    ISchoolClassRepository classRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateSchoolClassCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await classRepository.TeacherExistsAsync(request.HomeroomTeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "SchoolClass.TeacherNotFound",
                description: "Вказаного вчителя не знайдено.");
        }

        if (await classRepository.ExistsByNameAndYearExcludingIdAsync(request.ClassName, request.AcademicYear, request.ClassId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "SchoolClass.Duplicate",
                description: $"Клас '{request.ClassName}' на навчальний рік {request.AcademicYear} вже існує.");
        }

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var schoolClassToUpdate = new SchoolClass
        {
            ClassId = request.ClassId,
            ClassName = request.ClassName,
            GradeLevel = request.GradeLevel,
            AcademicYear = request.AcademicYear,
            HomeroomTeacherId = request.HomeroomTeacherId,
            RowVersion = rowVersionBytes
        };

        var updatedState = await classRepository.UpdateAsync(schoolClassToUpdate, cancellationToken).ConfigureAwait(false);

        if (updatedState is null)
        {
            return Error.Conflict(
                code: "SchoolClass.ConcurrencyOrNotFound",
                description: "Клас не знайдено, або його дані вже були змінені іншим користувачем. Будь ласка, оновіть сторінку.");
        }

        auditContext.TrackNewState(updatedState);

        return Result.Success;
    }
}