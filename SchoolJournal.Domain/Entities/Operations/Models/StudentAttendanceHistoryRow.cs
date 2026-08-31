using System;
using System.Collections.Generic;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record StudentAttendanceHistoryRow
{
    public DateTimeOffset LessonDate { get; init; }
    public string? LessonTopic { get; init; }
    public required string SubjectName { get; init; }
    public Guid AttendanceId { get; init; }
    public required string Status { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}