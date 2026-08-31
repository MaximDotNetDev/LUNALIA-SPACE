using System;
using System.Collections.Generic;

namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record SubjectStatsResponse(
    string SubjectName,
    int TotalLessons,
    int PresentCount,
    int AbsentCount,
    int LateCount,
    double AttendancePercentage
);

public sealed record StudentAttendanceStatsResponse(
    Guid StudentId,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    int TotalLessons,
    int TotalPresent,
    int TotalAbsent,
    int TotalLate,
    double OverallAttendancePercentage,
    IEnumerable<SubjectStatsResponse> SubjectStats
);