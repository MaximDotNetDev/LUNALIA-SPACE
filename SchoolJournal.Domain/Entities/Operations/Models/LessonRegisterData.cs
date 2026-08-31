using System;
using System.Collections.Generic;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record LessonRegisterData
{
    public Guid LessonId { get; init; }
    public string? LessonTopic { get; init; }
    public DateTimeOffset LessonDate { get; init; }
    public IEnumerable<LessonRegisterRow> Rows { get; init; } = [];
}