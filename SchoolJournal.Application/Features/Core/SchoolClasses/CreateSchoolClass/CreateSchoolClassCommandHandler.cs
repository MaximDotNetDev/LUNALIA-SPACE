using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.CreateSchoolClass;

public sealed class CreateSchoolClassCommandHandler(
    ISchoolClassRepository classRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateSchoolClassCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateSchoolClassCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await classRepository.TeacherExistsAsync(request.HomeroomTeacherId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "SchoolClass.TeacherNotFound",
                description: "Вказаного вчителя не знайдено.");
        }

        if (await classRepository.ExistsByNameAndYearAsync(request.ClassName, request.AcademicYear, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "SchoolClass.Duplicate",
                description: $"Клас '{request.ClassName}' на навчальний рік {request.AcademicYear} вже існує.");
        }

        var schoolClass = new SchoolClass
        {
            ClassName = request.ClassName,
            GradeLevel = request.GradeLevel,
            AcademicYear = request.AcademicYear,
            HomeroomTeacherId = request.HomeroomTeacherId,
            IsActive = true
        };

        var classId = await classRepository.AddAsync(schoolClass, cancellationToken).ConfigureAwait(false);

        var newState = await classRepository.GetByIdAsync(classId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return classId;
    }
}