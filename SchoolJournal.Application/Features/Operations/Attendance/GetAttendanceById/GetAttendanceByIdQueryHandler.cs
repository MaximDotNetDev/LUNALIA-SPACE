using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetAttendanceById;

public sealed class GetAttendanceByIdQueryHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAttendanceByIdQuery, ErrorOr<AttendanceResponse>>
{
    public async Task<ErrorOr<AttendanceResponse>> Handle(GetAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attendance = await attendanceRepository.GetByIdAsync(request.AttendanceId, cancellationToken).ConfigureAwait(false);

        if (attendance is null || attendance.IsDeleted)
        {
            return Error.NotFound(
                code: "Attendance.NotFound",
                description: $"Запис про відвідуваність з ID '{request.AttendanceId}' не знайдено.");
        }

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        switch (userRole)
        {
            case RoleType.Admin:
            case RoleType.Director:
                // Повний доступ до будь-яких аналітичних зрізів
                break;

            case RoleType.Teacher:
                var isTeacherAssigned = await attendanceRepository.IsTeacherAssignedToLessonAsync(currentUserId, attendance.LessonId, cancellationToken).ConfigureAwait(false);
                if (!isTeacherAssigned)
                {
                    return Error.Forbidden(
                        code: "Attendance.TeacherAccessDenied",
                        description: "У вас немає доступу до перегляду цього запису, оскільки ви не викладаєте на цьому уроці.");
                }
                break;

            case RoleType.Student:
                var isOwnRecord = await attendanceRepository.IsStudentUserAsync(currentUserId, attendance.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isOwnRecord)
                {
                    return Error.Forbidden(
                        code: "Attendance.StudentAccessDenied",
                        description: "Студент може переглядати виключно власну картку відвідуваності.");
                }
                break;

            case RoleType.Parent:
                var isChildRecord = await attendanceRepository.IsParentOfStudentUserAsync(currentUserId, attendance.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isChildRecord)
                {
                    return Error.Forbidden(
                        code: "Attendance.ParentAccessDenied",
                        description: "Батьки мають доступ до перегляду журналів тільки власних дітей.");
                }
                break;

            default:
                return Error.Forbidden(
                    code: "Attendance.RoleUnauthorized",
                    description: "Ваша роль у системі не має прав для виконання цієї операції.");
        }

        var response = new AttendanceResponse(
            attendance.AttendanceId,
            attendance.LessonId,
            attendance.StudentId,
            attendance.Status,
            attendance.Comment,
            attendance.CreatedAt,
            attendance.UpdatedAt,
            Convert.ToBase64String(attendance.RowVersion.ToArray())
        );

        return response;
    }
}