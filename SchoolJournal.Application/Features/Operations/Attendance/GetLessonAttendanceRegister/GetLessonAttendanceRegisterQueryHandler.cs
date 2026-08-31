using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;
using SchoolJournal.Domain.Entities.Operations.Models;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetLessonAttendanceRegister;

public sealed class GetLessonAttendanceRegisterQueryHandler(
    IAttendanceRepository attendanceRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLessonAttendanceRegisterQuery, ErrorOr<LessonAttendanceRegisterResponse>>
{
    public async Task<ErrorOr<LessonAttendanceRegisterResponse>> Handle(GetLessonAttendanceRegisterQuery request, CancellationToken cancellationToken)
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
                    code: "Register.TeacherAccessDenied",
                    description: "Ви не маєте доступу до відомості цього уроку, оскільки не є призначеним викладачем чи замісником.");
            }
        }

        var registerData = await attendanceRepository.GetLessonRegisterInternalAsync(request.LessonId, cancellationToken).ConfigureAwait(false);

        if (registerData is null)
        {
            return Error.NotFound(
                code: "Register.LessonNotFound",
                description: $"Урок з ID '{request.LessonId}' не знайдено або видалено.");
        }

        static IEnumerable<RegisterStudentRow> MapToRows(IEnumerable<SchoolJournal.Domain.Entities.Operations.Models.LessonRegisterRow> domainRows)
        {
            return domainRows.Select(r => new RegisterStudentRow(
                r.StudentId,
                r.LastName,
                r.FirstName,
                r.MiddleName,
                r.AttendanceId,
                r.Status,
                r.Comment,
                r.RowVersion is not null && r.RowVersion.Count > 0 ? Convert.ToBase64String(r.RowVersion.ToArray()) : null
            ));
        }

        var response = new LessonAttendanceRegisterResponse(
            registerData.LessonId,
            registerData.LessonTopic,
            registerData.LessonDate,
            MapToRows(registerData.Rows)
        );

        return response;
    }
}