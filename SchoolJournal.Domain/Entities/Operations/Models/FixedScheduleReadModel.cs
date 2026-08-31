using SchoolJournal.Domain.Enums;

namespace SchoolJournal.Domain.Entities.Operations.Models;

public sealed record FixedScheduleReadModel
{
    public Guid ScheduleId { get; init; }
    public SchoolDayOfWeek DayOfWeek { get; init; }
    public Guid PeriodId { get; init; }
    public string PeriodName { get; init; } = string.Empty;
    public Guid AssignmentId { get; init; }
    public string AssignmentDetails { get; init; } = string.Empty;
    public Guid RoomId { get; init; }
    public string RoomName { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public IReadOnlyCollection<byte> RowVersion { get; init; } = [];
}