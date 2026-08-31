namespace SchoolJournal.Contracts.DTOs.Operations.FixedSchedules;

public sealed record UpdateFixedScheduleRequest(
    int DayOfWeek,
    Guid PeriodId,
    Guid AssignmentId,
    Guid RoomId,
    string RowVersionBase64
);