using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryQueryHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetStudentAttendanceHistoryQuery, ErrorOr<StudentAttendanceHistoryResponse>>
{
    public async Task<ErrorOr<StudentAttendanceHistoryResponse>> Handle(GetStudentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await attendanceRepository.StudentExistsAsync(request.StudentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "History.StudentNotFound",
                description: $"Студента з ID '{request.StudentId}' не знайдено або він заархівований.");
        }

        var userRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        switch (userRole)
        {
            case RoleType.Admin:
            case RoleType.Director:
                break;

            case RoleType.Teacher:
                var isTeacherLinked = await attendanceRepository.IsTeacherAssignedToStudentsClassAsync(currentUserId, request.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isTeacherLinked)
                {
                    return Error.Forbidden(
                        code: "History.TeacherAccessDenied",
                        description: "Ви можете переглядати історію відвідуваності лише тих студентів, у чиїх класах ви офіційно викладаєте або проводите заміщення.");
                }
                break;

            case RoleType.Student:
                var isOwnProfile = await attendanceRepository.IsStudentUserAsync(currentUserId, request.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isOwnProfile)
                {
                    return Error.Forbidden(
                        code: "History.StudentAccessDenied",
                        description: "Студент має право запитувати історію виключно свого власного профілю.");
                }
                break;

            case RoleType.Parent:
                var isChildProfile = await attendanceRepository.IsParentOfStudentUserAsync(currentUserId, request.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isChildProfile)
                {
                    return Error.Forbidden(
                        code: "History.ParentAccessDenied",
                        description: "Батьки мають право на перегляд аналітичних зрізів виключно своїх дітей.");
                }
                break;

            default:
                return Error.Forbidden(
                    code: "History.RoleUnauthorized",
                    description: "Ваша роль у системі не має прав для доступу до історичних журналів відвідуваності.");
        }

        var historyData = await attendanceRepository.GetStudentHistoryInternalAsync(
            request.StudentId,
            request.StartDate,
            request.EndDate,
            cancellationToken).ConfigureAwait(false);

        static IEnumerable<HistoryAttendanceRowResponse> MapToResponse(IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.StudentAttendanceHistoryRow> domainRows)
        {
            return domainRows.Select(r => new HistoryAttendanceRowResponse(
                r.LessonDate,
                r.LessonTopic,
                r.SubjectName,
                r.AttendanceId,
                r.Status,
                r.Comment,
                Convert.ToBase64String(r.RowVersion.ToArray())
            ));
        }

        return new StudentAttendanceHistoryResponse(
            StudentId: request.StudentId,
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            History: MapToResponse(historyData)
        );
    }
}