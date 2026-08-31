using System;
using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceHistory;

public sealed record GetStudentAttendanceHistoryQuery(
    Guid StudentId,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IRequest<ErrorOr<StudentAttendanceHistoryResponse>>;