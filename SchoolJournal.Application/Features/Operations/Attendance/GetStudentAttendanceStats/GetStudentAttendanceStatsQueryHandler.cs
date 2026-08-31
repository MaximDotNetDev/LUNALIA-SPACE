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

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceStats;

public sealed class GetStudentAttendanceStatsQueryHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetStudentAttendanceStatsQuery, ErrorOr<StudentAttendanceStatsResponse>>
{
    public async Task<ErrorOr<StudentAttendanceStatsResponse>> Handle(GetStudentAttendanceStatsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await attendanceRepository.StudentExistsAsync(request.StudentId, cancellationToken).ConfigureAwait(false))
        {
            return Error.NotFound(
                code: "Stats.StudentNotFound",
                description: $"Студента з ID '{request.StudentId}' не знайдено в системі.");
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
                        code: "Stats.TeacherAccessDenied",
                        description: "Ви маєте доступ до перегляду статистики лише тих учнів, у чиїх класах ви проводите заняття або заміщення.");
                }
                break;

            case RoleType.Student:
                var isOwnProfile = await attendanceRepository.IsStudentUserAsync(currentUserId, request.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isOwnProfile)
                {
                    return Error.Forbidden(
                        code: "Stats.StudentAccessDenied",
                        description: "Користувач-студент може запитувати аналітику виключно по власному профілю.");
                }
                break;

            case RoleType.Parent:
                var isChildProfile = await attendanceRepository.IsParentOfStudentUserAsync(currentUserId, request.StudentId, cancellationToken).ConfigureAwait(false);
                if (!isChildProfile)
                {
                    return Error.Forbidden(
                        code: "Stats.ParentAccessDenied",
                        description: "Доступ відхилено. Батьки можуть переглядати аналітичні дані лише своїх дітей.");
                }
                break;

            default:
                return Error.Forbidden(
                    code: "Stats.RoleUnauthorized",
                    description: "Ваша системна роль не володіє правами для формування звітів відвідуваності.");
        }

        var statsData = await attendanceRepository.GetStudentStatsInternalAsync(
            request.StudentId,
            request.StartDate,
            request.EndDate,
            cancellationToken).ConfigureAwait(false);

        // Повністю ізольована static функція для безпечного обчислення відсотків успішності
        static double CalculatePercentage(int attended, int total)
        {
            if (total <= 0)
            {
                return 0.0;
            }
            return Math.Round((double)attended / total * 100, 2);
        }

        var overallPercentage = CalculatePercentage(statsData.TotalPresent + statsData.TotalLate, statsData.TotalLessons);

        var subjectStatsResponses = statsData.Subjects.Select(s => new SubjectStatsResponse(
            s.SubjectName,
            s.TotalLessons,
            s.PresentCount,
            s.AbsentCount,
            s.LateCount,
            CalculatePercentage(s.PresentCount + s.LateCount, s.TotalLessons)
        )).ToList();

        return new StudentAttendanceStatsResponse(
            request.StudentId,
            request.StartDate,
            request.EndDate,
            statsData.TotalLessons,
            statsData.TotalPresent,
            statsData.TotalAbsent,
            statsData.TotalLate,
            overallPercentage,
            subjectStatsResponses
        );
    }
}