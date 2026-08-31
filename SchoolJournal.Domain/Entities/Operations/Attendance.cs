namespace SchoolJournal.Domain.Entities.Operations;

public sealed record Attendance
{
    public Guid AttendanceId { get; init; }

    public Guid LessonId { get; init; }

    public Guid StudentId { get; init; }

    public required string Status { get; init; }

    public string? Comment { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}