namespace SchoolJournal.Domain.Entities.Reference;

public sealed record BellSchedule
{
    public Guid ScheduleId { get; init; }

    public int LessonNumber { get; init; }

    public DateTimeOffset StartTime { get; init; }

    public DateTimeOffset EndTime { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}