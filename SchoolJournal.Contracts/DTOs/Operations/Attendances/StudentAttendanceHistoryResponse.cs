using System;
using System.Collections.Generic;

namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record HistoryAttendanceRowResponse(
    DateTimeOffset LessonDate,
    string? LessonTopic,
    string SubjectName,
    Guid AttendanceId,
    string Status,
    string? Comment,
    string RowVersionBase64
);

public sealed record StudentAttendanceHistoryResponse(
    Guid StudentId,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    IEnumerable<HistoryAttendanceRowResponse> History
);