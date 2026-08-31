namespace SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

public sealed record AssignHomeroomTeacherRequest(
    Guid NewHomeroomTeacherId,
    string RowVersionBase64
);