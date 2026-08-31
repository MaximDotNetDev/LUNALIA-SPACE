using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.Attendance.UpdateAttendance;

public sealed record UpdateAttendanceCommand(
    Guid AttendanceId,
    string Status,
    string? Comment,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;