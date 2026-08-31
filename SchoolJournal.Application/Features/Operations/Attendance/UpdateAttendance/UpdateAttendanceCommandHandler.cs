using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateAttendanceCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attendance = await attendanceRepository.GetByIdAsync(request.AttendanceId, cancellationToken).ConfigureAwait(false);

        if (attendance is null || attendance.IsDeleted)
        {
            return Error.NotFound(
                code: "Attendance.NotFound",
                description: $"Запис про відвідуваність з ID '{request.AttendanceId}' не знайдено або видалено.");
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
                    description: "Ви не маєте право редагувати відвідуваність на цьому уроці, оскільки не є його викладачем або замінником.");
            }
        }

        auditContext.TrackOldState(attendance);

        var rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);

        var updatedAttendance = attendance with
        {
            Status = request.Status,
            Comment = request.Comment,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersionBytes
        };

        var result = await attendanceRepository.UpdateAsync(updatedAttendance, cancellationToken).ConfigureAwait(false);

        if (result is null)
        {
            return Error.Conflict(
                code: "Attendance.ConcurrencyConflict",
                description: "Запис було змінено іншим користувачем або видалено. Перезавантажте сторінку.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}