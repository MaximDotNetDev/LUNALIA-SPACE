namespace SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

public sealed record CreateFixedScheduleRequest(
    int DayOfWeek,
    Guid PeriodId,
    Guid AssignmentId,
    Guid RoomId
);