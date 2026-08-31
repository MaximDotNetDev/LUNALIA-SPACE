namespace SchoolJournal.Contracts.DTOs.Operations.Attendances;

public sealed record DeleteAttendanceRequest(
    string RowVersionBase64
);