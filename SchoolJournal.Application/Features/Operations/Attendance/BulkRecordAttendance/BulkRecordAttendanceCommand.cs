using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;

namespace SchoolJournal.Application.Features.Operations.Attendance.BulkRecordAttendance;

public sealed record BulkRecordAttendanceCommand(
    Guid LessonId,
    IEnumerable<StudentAttendanceItem> Students
) : IRequest<ErrorOr<Success>>;