using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.RecordAttendance;

public sealed class RecordAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<RecordAttendanceCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userRole = currentUserService.GetUserRole();
        if (userRole == RoleType.Teacher)
        {
            var teacherId = currentUserService.GetUserId();
            var isAssigned = await attendanceRepository.IsTeacherAssignedToLessonAsync(teacherId, request.LessonId, cancellationToken).ConfigureAwait(false);

            if (!isAssigned)
            {
                return Error.Forbidden(
                    code: "Attendance.TeacherNotAssigned",
                    description: "Ви не маєте прав для виставлення відвідуваності на цей урок, оскільки не є його викладачем або замінником.");
            }
        }

        if (!await attendanceRepository.LessonExistsAsync(request.LessonId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Attendance.LessonNotFound",
                description: $"Урок з ID '{request.LessonId}' не знайдено.");
        }

        if (!await attendanceRepository.StudentExistsAsync(request.StudentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Attendance.StudentNotFound",
                description: $"Студента з ID '{request.StudentId}' не знайдено.");
        }

        if (await attendanceRepository.ExistsByLessonAndStudentAsync(request.LessonId, request.StudentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "Attendance.AlreadyExists",
                description: "Запис про відвідуваність цього студента на цьому уроці вже існує. Використовуйте оновлення.");
        }

        var attendance = new Domain.Entities.Operations.Attendance
        {
            AttendanceId = Guid.NewGuid(),
            LessonId = request.LessonId,
            StudentId = request.StudentId,
            Status = request.Status,
            Comment = request.Comment,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        var attendanceId = await attendanceRepository.AddAsync(attendance, cancellationToken).ConfigureAwait(false);

        var newState = await attendanceRepository.GetByIdAsync(attendanceId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return attendanceId;
    }
}