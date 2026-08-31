using System;
using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Attendances;

namespace SchoolJournal.Application.Features.Operations.Attendance.GetStudentAttendanceStats;

public sealed record GetStudentAttendanceStatsQuery(
    Guid StudentId,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
) : IRequest<ErrorOr<StudentAttendanceStatsResponse>>;