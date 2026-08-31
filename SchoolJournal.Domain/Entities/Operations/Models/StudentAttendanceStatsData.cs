using System;
using System.Collections.Generic;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record StudentAttendanceStatsData
{
    public int TotalLessons { get; init; }
    public int TotalPresent { get; init; }
    public int TotalAbsent { get; init; }
    public int TotalLate { get; init; }
    public IEnumerable<SubjectAttendanceStats> Subjects { get; init; } = [];
}