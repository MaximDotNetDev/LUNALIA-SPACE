using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Attendance.SoftDeleteAttendance;

public sealed record SoftDeleteAttendanceCommand(
    Guid AttendanceId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;