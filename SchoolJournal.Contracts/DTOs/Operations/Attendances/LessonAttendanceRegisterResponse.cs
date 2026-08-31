using System;
using System.Collections.Generic;

namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record RegisterStudentRow(
    Guid StudentId,
    string LastName,
    string FirstName,
    string? MiddleName,
    Guid? AttendanceId,
    string? Status,
    string? Comment,
    string? RowVersionBase64
);

public sealed record LessonAttendanceRegisterResponse(
    Guid LessonId,
    string? LessonTopic,
    DateTimeOffset LessonDate,
    IEnumerable<RegisterStudentRow> Students
);