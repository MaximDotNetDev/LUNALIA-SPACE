using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherAcademicInfo;

public sealed class UpdateTeacherAcademicInfoCommandHandler(
    ITeacherRepository teacherRepository,
    IAuditContext auditContext)
    : IRequestHandler<UpdateTeacherAcademicInfoCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateTeacherAcademicInfoCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentTeacher = await teacherRepository.GetByIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);
        if (currentTeacher is null)
        {
            return Error.NotFound("Teacher.NotFound", "Профіль вчителя не знайдено.");
        }

        auditContext.TrackOldState(currentTeacher);

        var rowVersion = Convert.FromBase64String(request.RowVersionBase64);
        var updatedTeacher = currentTeacher with
        {
            PositionId = request.PositionId,
            QualificationId = request.QualificationId,
            PedagogicalTitleId = request.PedagogicalTitleId,
            Workload = request.Workload,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersion
        };

        var result = await teacherRepository.UpdateAcademicInfoAsync(updatedTeacher, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict("Teacher.ConcurrencyConflict", "Дані були змінені або версія застаріла.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}