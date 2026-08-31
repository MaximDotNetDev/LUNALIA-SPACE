namespace SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

public sealed record FixedScheduleResponse(
    Guid ScheduleId,
    int DayOfWeek,
    Guid PeriodId,
    string PeriodName,
    Guid AssignmentId,
    string AssignmentDetails,
    Guid RoomId,
    string RoomName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string RowVersionBase64
);