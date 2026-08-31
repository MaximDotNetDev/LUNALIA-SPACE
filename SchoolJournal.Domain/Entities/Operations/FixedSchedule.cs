using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Domain.Entities.Operations;

public sealed record FixedSchedule
{
    public Guid ScheduleId { get; init; }

    public SchoolDayOfWeek DayOfWeek { get; init; }

    public Guid PeriodId { get; init; }

    public Guid AssignmentId { get; init; }

    public Guid RoomId { get; init; }

    public bool IsDeleted { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}