using System;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record SubjectAttendanceStats
{
    public required string SubjectName { get; init; }
    public int TotalLessons { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
}