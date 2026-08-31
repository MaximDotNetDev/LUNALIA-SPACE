using System;
using System.Collections.Generic;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record LessonRegisterRow
{
    public Guid StudentId { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public Guid? AttendanceId { get; init; }
    public string? Status { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyCollection<byte>? RowVersion { get; init; } = [];
}