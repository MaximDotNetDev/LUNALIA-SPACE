using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetAttendanceById;

public sealed record GetAttendanceByIdQuery(
    Guid AttendanceId
) : IRequest<ErrorOr<AttendanceResponse>>;