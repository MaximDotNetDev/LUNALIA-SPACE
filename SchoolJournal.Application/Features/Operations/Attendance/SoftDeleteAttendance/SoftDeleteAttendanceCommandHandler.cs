using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.SoftDeleteAttendance;

public sealed class SoftDeleteAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<SoftDeleteAttendanceCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(SoftDeleteAttendanceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attendance = await attendanceRepository.GetByIdAsync(request.AttendanceId, cancellationToken).ConfigureAwait(false);

        if (attendance is null || attendance.IsDeleted)
        {
            return Error.NotFound(
                code: "Attendance.NotFound",
                description: $"Запис про відвідуваність з ID '{request.AttendanceId}' не знайдено або вже видалено.");
        }

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var teacherId = currentUserService.GetUserId();
            var isAssigned = await attendanceRepository.IsTeacherAssignedToLessonAsync(teacherId, attendance.LessonId, cancellationToken).ConfigureAwait(false);

            if (!isAssigned)
            {
                return Error.Forbidden(
                    code: "Attendance.TeacherNotAssigned",
                    description: "Ви не маєте прав для видалення відвідуваності на цьому уроці.");
            }
        }

        auditContext.TrackOldState(attendance);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var deletedAttendance = await attendanceRepository.SoftDeleteAsync(request.AttendanceId, rowVersionBytes, cancellationToken).ConfigureAwait(false);

        if (deletedAttendance is null)
        {
            return Error.Conflict(
                code: "Attendance.ConcurrencyConflict",
                description: "Запис було змінено або видалено іншим користувачем. Перезавантажте сторінку.");
        }

        auditContext.TrackNewState(deletedAttendance);

        return Result.Success;
    }
}