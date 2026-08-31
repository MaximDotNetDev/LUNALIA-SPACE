using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetLessonAttendanceRegister;

public sealed record GetLessonAttendanceRegisterQuery(
    Guid LessonId
) : IRequest<ErrorOr<LessonAttendanceRegisterResponse>>;