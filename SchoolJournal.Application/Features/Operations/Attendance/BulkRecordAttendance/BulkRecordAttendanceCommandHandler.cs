using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.BulkRecordAttendance;

public sealed class BulkRecordAttendanceCommandHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<BulkRecordAttendanceCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(BulkRecordAttendanceCommand request, CancellationToken cancellationToken)
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
                    description: "Ви не маєте прав для масового виставлення відвідуваності на цей урок.");
            }
        }

        if (!await attendanceRepository.LessonExistsAsync(request.LessonId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Attendance.LessonNotFound",
                description: $"Урок з ID '{request.LessonId}' не знайдено.");
        }

        var oldAttendances = await attendanceRepository.GetByLessonIdAsync(request.LessonId, cancellationToken).ConfigureAwait(false);
        auditContext.TrackOldState(oldAttendances);

        var domainAttendances = request.Students.Select(s => new Domain.Entities.Operations.Attendance
        {
            AttendanceId = Guid.NewGuid(),
            LessonId = request.LessonId,
            StudentId = s.StudentId,
            Status = s.Status,
            Comment = s.Comment,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        }).ToList();

        await attendanceRepository.BulkUpsertAsync(request.LessonId, domainAttendances, cancellationToken).ConfigureAwait(false);

        var newAttendances = await attendanceRepository.GetByLessonIdAsync(request.LessonId, cancellationToken).ConfigureAwait(false);
        auditContext.TrackNewState(newAttendances);

        return Result.Success;
    }
}