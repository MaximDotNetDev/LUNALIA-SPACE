namespace SchoolJournal.Domain.Entities.Operations;

public sealed record Lesson
{
    public Guid LessonId { get; init; }

    public Guid AssignmentId { get; init; }

    public DateTimeOffset LessonDate { get; init; }

    public string? LessonTopic { get; init; }

    public string? Homework { get; init; }

    public Guid LessonTypeId { get; init; }

    public Guid PeriodId { get; init; }

    public Guid RoomId { get; init; }

    public Guid SemesterId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}